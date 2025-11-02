using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only logged-in teachers can access the teacher dashboard
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TeacherController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /Teacher/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // ---------- 1) Teacher info from session ----------
            var teacherId = HttpContext.Session.GetInt32("UserID");
            var teacherName = HttpContext.Session.GetString("UserName");
            var teacherEmail = HttpContext.Session.GetString("UserEmail");

            if (teacherId == null)
            {
                TempData["Error"] = "Please log in first.";
                return RedirectToAction("LoginTeacher", "Auth");
            }

            ViewBag.TeacherName = teacherName ?? "Unknown Teacher";
            ViewBag.TeacherEmail = teacherEmail ?? "unknown@mail.com";

            // ---------- 2) Fetch teacher courses from API ----------
            var courses = new List<TeacherCourseRowView>();

            try
            {
                var response = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiCourses = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

                    foreach (var c in apiCourses)
                    {
                        courses.Add(new TeacherCourseRowView
                        {
                            CourseID = c.CourseID,
                            CourseCode = c.CourseCode,
                            CourseName = c.CourseName,
                            DepartmentName = "",  // fill later if your API sends it
                            SemesterName = "",
                            StudentCount = 0,
                            AverageGrade = 0,
                            AttendanceRate = 0
                        });
                    }
                }
                else
                {
                    ViewBag.Error = $"API error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not load courses from API: " + ex.Message;
            }

            // ---------- 3) Build the TeacherDashboardView ----------
            var dashboard = new TeacherDashboardView
            {
                TeacherName = teacherName ?? "Teacher",
                TotalCourses = courses.Count,
                TotalStudents = courses.Sum(c => c.StudentCount),
                AverageGrade = courses.Any() ? courses.Average(c => c.AverageGrade) : 0,
                AttendanceRate = courses.Any() ? courses.Average(c => c.AttendanceRate) : 0,
                Courses = courses,
                RecentActivities = new List<RecentActivityView>
                {
                    new() { Description = "Added new test", Date = DateTime.Now.AddDays(-2) },
                    new() { Description = "Updated grades", Date = DateTime.Now.AddDays(-1) },
                    new() { Description = "Recorded attendance", Date = DateTime.Now }
                }
            };

            // ---------- 4) Return dashboard to the view ----------
            return View("~/Views/Teacher/Dashboard.cshtml", dashboard);
        }
    }
}
