using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Students must be logged in to see their dashboard
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentDashboardController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl")?.Value ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? semester = null, string? department = null)
        {
            // ✅ 1) Get current student ID from session
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "Please log in first.";
                return RedirectToAction("Login", "Auth");
            }

            // ✅ 2) Fetch enrolled courses via API
            var courseCards = new List<CourseCardVM>();
            var enrolledRes = await _client.GetAsync($"{_apiBase}StudentCourses/user/{userId}");
            if (enrolledRes.IsSuccessStatusCode)
            {
                var json = await enrolledRes.Content.ReadAsStringAsync();
                var enrolled = JsonConvert.DeserializeObject<List<StudentCourseVM>>(json) ?? new();

                foreach (var c in enrolled)
                {
                    double attendanceRate = 0;
                    double avg = 0;

                    // Attendance %
                    var attRes = await _client.GetAsync($"{_apiBase}Attendance/user/{userId}/course/{c.CourseID}/percent");
                    if (attRes.IsSuccessStatusCode &&
                        double.TryParse(await attRes.Content.ReadAsStringAsync(), out var att))
                        attendanceRate = att;

                    // Average grade
                    var avgRes = await _client.GetAsync($"{_apiBase}TestGrades/user/{userId}/course/{c.CourseID}/average");
                    if (avgRes.IsSuccessStatusCode &&
                        double.TryParse(await avgRes.Content.ReadAsStringAsync(), out var avgScore))
                        avg = avgScore;

                    courseCards.Add(new CourseCardVM
                    {
                        CourseID = c.CourseID,
                        CourseName = c.CourseName,
                        TeacherName = c.TeacherName,
                        Department = c.Department,
                        Semester = c.Semester,
                        AttendanceRate = attendanceRate,
                        CurrentAverage = avg
                    });
                }
            }

            // ✅ 3) Filters
            var allDepartments = courseCards.Select(x => x.Department).Distinct().ToList();
            var allSemesters = courseCards.Select(x => x.Semester).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(department))
                courseCards = courseCards.Where(c => c.Department == department).ToList();
            if (!string.IsNullOrWhiteSpace(semester))
                courseCards = courseCards.Where(c => c.Semester == semester).ToList();

            // ✅ 4) GPA + Global Attendance
            double gpa = 0;
            double overallAttendance = 0;

            var gpaRes = await _client.GetAsync($"{_apiBase}TestGrades/user/{userId}/gpa");
            if (gpaRes.IsSuccessStatusCode &&
                double.TryParse(await gpaRes.Content.ReadAsStringAsync(), out var g))
                gpa = g;

            var attOverallRes = await _client.GetAsync($"{_apiBase}Attendance/user/{userId}/percent");
            if (attOverallRes.IsSuccessStatusCode &&
                double.TryParse(await attOverallRes.Content.ReadAsStringAsync(), out var attPercent))
                overallAttendance = attPercent;

            // ✅ 5) Notifications (sample or fetched later)
            var notifications = new List<NotificationVM>
            {
                new() { Message = "Welcome to your dashboard!", Type = "Info", CreatedAt = DateTime.Now },
                new() { Message = $"You are enrolled in {courseCards.Count} courses.", Type = "Success", CreatedAt = DateTime.Now }
            };

            // ✅ 6) Simple chart demo data (optional)
            var gradeSeries = new List<SeriesPointVM>
            {
                new() { X = DateTime.Now.AddDays(-10), Y = 75 },
                new() { X = DateTime.Now.AddDays(-5),  Y = 80 },
                new() { X = DateTime.Now,              Y = 85 }
            };
            var attendanceSeries = new List<SeriesPointVM>
            {
                new() { X = DateTime.Now.AddDays(-10), Y = 90 },
                new() { X = DateTime.Now.AddDays(-5),  Y = 88 },
                new() { X = DateTime.Now,              Y = 92 }
            };

            // ✅ 7) Final view model
            var vm = new StudentDashboardVM
            {
                UserID = userId.Value,
                StudentName = HttpContext.Session.GetString("UserName") ?? "Student",
                CurrentSemester = allSemesters.LastOrDefault() ?? "-",
                ActiveCoursesCount = courseCards.Count,
                GPA = gpa,
                AttendancePercent = overallAttendance,
                Courses = courseCards,
                Notifications = notifications,
                GradeSeries = gradeSeries,
                AttendanceSeries = attendanceSeries,
                Departments = allDepartments,
                Semesters = allSemesters,
                SelectedDepartment = department,
                SelectedSemester = semester
            };

            return View(vm);
        }
    }
}
