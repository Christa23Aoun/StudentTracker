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
            var student = JsonSerializer.Deserialize<UserView>(studentJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var depRes = await client.GetAsync("departments");
            if (!depRes.IsSuccessStatusCode)
                return RedirectToAction("Index", "Users");

            var depJson = await depRes.Content.ReadAsStringAsync();
            var deps = JsonSerializer.Deserialize<List<DepartmentView>>(depJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var enrollRes = await client.GetAsync($"Enrollments/student/{studentId}");
            var enrollJson = await enrollRes.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null
            };

            var enrollments = JsonSerializer.Deserialize<List<EnrollmentCourseView>>(enrollJson, options) ?? new();

            var active = enrollments.Where(e => e.IsActive).Select(e => e.CourseID).ToList();
            var dropped = enrollments.Where(e => !e.IsActive).Select(e => e.CourseID).ToList();

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

            Console.WriteLine("ENROLLED: " + string.Join(",", active));
            Console.WriteLine("DROPPED: " + string.Join(",", dropped));

            var vm = new EnrollmentViewModel
            {
                StudentID = studentId,
                StudentName = student?.FullName ?? "Unknown",
                Departments = departmentCourses,
                EnrolledCourseIds = active,
                DroppedCourseIds = dropped,
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
                TempData["Msg"] = "⚠️ No courses selected.";
                return RedirectToAction("Enroll", new { studentId = model.StudentID });
            }

            var client = Api();
            bool allSuccess = true;

            foreach (var courseId in model.SelectedCourses)
            {
                var body = JsonSerializer.Serialize(new { StudentID = model.StudentID, CourseID = courseId });
                var payload = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync("Enrollments/enroll", payload);
                if (!res.IsSuccessStatusCode) allSuccess = false;
            }

            TempData["Msg"] = allSuccess ? "✅ Enrollments updated." : "⚠️ Some enrollments failed.";

            return RedirectToAction("Index", "Users", new { role = model.ReturnRole, status = model.ReturnStatus });
        }

        [HttpPost]
        public async Task<IActionResult> Unenroll(int studentId, int courseId)
        {
            var client = Api();
            var body = JsonSerializer.Serialize(new { StudentID = studentId, CourseID = courseId });
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("Enrollments/unenroll", UriKind.Relative),
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            await client.SendAsync(request);
            return RedirectToAction("Enroll", new { studentId });
        }

        [HttpPost]
        public async Task<IActionResult> ReEnroll(int studentId, int courseId)
        {
            var client = Api();
            var body = JsonSerializer.Serialize(new { StudentID = studentId, CourseID = courseId });
            var payload = new StringContent(body, Encoding.UTF8, "application/json");
            await client.PostAsync("Enrollments/reenroll", payload);

            return RedirectToAction("Enroll", new { studentId });
        }
    }
}
