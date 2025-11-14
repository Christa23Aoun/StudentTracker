using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Models;
using System.Text;
using System.Text.Json;

namespace StudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EnrollmentsController : BaseController
    {
        private readonly IHttpClientFactory _http;

        public EnrollmentsController(IHttpClientFactory http)
        {
            _http = http;
        }

        private HttpClient Api() => _http.CreateClient("API");

        [HttpGet]
        public async Task<IActionResult> Enroll(int studentId, string? role, string? status)
        {
            var client = Api();

            var studentRes = await client.GetAsync($"users/{studentId}");
            if (!studentRes.IsSuccessStatusCode)
                return RedirectToAction("Index", "Users");

            var studentJson = await studentRes.Content.ReadAsStringAsync();
            var student = JsonSerializer.Deserialize<UserView>(studentJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var depRes = await client.GetAsync("departments");
            if (!depRes.IsSuccessStatusCode)
                return RedirectToAction("Index", "Users");

            var depJson = await depRes.Content.ReadAsStringAsync();
            var deps = JsonSerializer.Deserialize<List<DepartmentView>>(depJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            var enrolledRes = await client.GetAsync($"StudentCourses");
            var enrolledJson = await enrolledRes.Content.ReadAsStringAsync();
            var enrolledCourses = JsonSerializer.Deserialize<List<StudentCourseView>>(enrolledJson) ?? new();

            var studentEnrolled = enrolledCourses
                .Where(x => x.StudentID == studentId)
                .Select(x => x.CourseID)
                .ToList();

            var departmentCourses = new List<DepartmentCoursesView>();
            foreach (var d in deps)
            {
                var depCourses = new DepartmentCoursesView
                {
                    DepartmentName = d.DepartmentName,
                    Courses = new List<SimpleCourseView>()
                };

                if (d.Courses != null)
                {
                    foreach (var c in d.Courses)
                    {
                        depCourses.Courses.Add(new SimpleCourseView
                        {
                            CourseID = c.CourseID,
                            CourseName = c.CourseName,
                            DepartmentName = d.DepartmentName
                        });
                    }
                }

                departmentCourses.Add(depCourses);
            }

            var vm = new EnrollmentViewModel
            {
                StudentID = studentId,
                StudentName = student?.FullName ?? "Unknown",
                Departments = departmentCourses,
                EnrolledCourseIds = studentEnrolled,
                ReturnRole = role,
                ReturnStatus = status
            };

            return View("~/Views/Enrollments/Enroll.cshtml", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollmentViewModel model)
        {
            if (model.SelectedCourses == null || !model.SelectedCourses.Any())
            {
                TempData["Msg"] = "⚠️ No courses selected for enrollment.";
                return RedirectToAction("Enroll", new { studentId = model.StudentID });
            }

            var client = Api();
            bool allSuccess = true;

            foreach (var courseId in model.SelectedCourses)
            {
                var body = JsonSerializer.Serialize(new
                {
                    StudentID = model.StudentID,
                    CourseID = courseId,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true
                });

                var payload = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync("StudentCourses/enroll", payload);

                if (!res.IsSuccessStatusCode)
                    allSuccess = false;
            }

            TempData["Msg"] = allSuccess
                ? "✅ Student successfully enrolled in selected courses."
                : "⚠️ Some enrollments failed.";

            return RedirectToAction("Index", "Users", new
            {
                role = model.ReturnRole,
                status = model.ReturnStatus
            });
        }
    }
}
