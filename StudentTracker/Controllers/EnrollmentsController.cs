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
            var student = JsonSerializer.Deserialize<UserView>(
                studentJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var depRes = await client.GetAsync("departments");
            if (!depRes.IsSuccessStatusCode)
                return RedirectToAction("Index", "Users");

            var depJson = await depRes.Content.ReadAsStringAsync();
            var deps = JsonSerializer.Deserialize<List<DepartmentView>>(
                depJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();

            var enrollRes = await client.GetAsync($"Enrollments/student/{studentId}");
            var enrollJson = await enrollRes.Content.ReadAsStringAsync();

            var enrollments = JsonSerializer.Deserialize<List<EnrollmentCourseView>>(
                enrollJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();

            var active = enrollments.Where(e => e.IsActive).Select(e => e.CourseID).ToList();

            var departmentCourses = new List<DepartmentCoursesView>();
            foreach (var d in deps)
            {
                var dep = new DepartmentCoursesView
                {
                    DepartmentName = d.DepartmentName,
                    Courses = new()
                };

                if (d.Courses != null)
                {
                    foreach (var c in d.Courses)
                    {
                        dep.Courses.Add(new SimpleCourseView
                        {
                            CourseID = c.CourseID,
                            CourseName = c.CourseName,
                            DepartmentName = d.DepartmentName
                        });
                    }
                }

                departmentCourses.Add(dep);
            }

            var vm = new EnrollmentViewModel
            {
                StudentID = studentId,
                StudentName = student?.FullName ?? "Unknown",
                Departments = departmentCourses,
                EnrolledCourseIds = active,
                ReturnRole = role,
                ReturnStatus = status
            };

            return View("~/Views/Enrollments/Enroll.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollmentViewModel model)
        {
            var client = Api();

            if (model.SelectedCourses != null)
            {
                foreach (var courseId in model.SelectedCourses)
                {
                    var json = JsonSerializer.Serialize(new { StudentID = model.StudentID, CourseID = courseId });
                    var body = new StringContent(json, Encoding.UTF8, "application/json");

                    await client.PostAsync("Enrollments/enroll", body);
                }
            }

            return RedirectToAction(
                "Index",
                "Users",
                new { role = "Student", status = "All" }
            );
        }

        [HttpPost]
        public async Task<IActionResult> Unenroll(int studentId, int courseId, string? role, string? status)
        {
            var client = Api();

            var json = JsonSerializer.Serialize(new { StudentID = studentId, CourseID = courseId });
            var body = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("Enrollments/unenroll", UriKind.Relative),
                Content = body
            };

            await client.SendAsync(request);

            return RedirectToAction("Enroll", new
            {
                studentId,
                role,
                status
            });
        }
    }
}
