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
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /StudentDashboard
        public async Task<IActionResult> Index(string? semester = null, string? department = null)
        {
            // 🔹 Get current student ID from session
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "Please log in first.";
                return RedirectToAction("Login", "Auth");
            }

            // ---------- 1) Enrolled courses ----------
            var enrolledRes = await _client.GetAsync($"{_apiBase}StudentCourses/user/{userId}");
            var enrolled = new List<StudentCourseVM>();
            if (enrolledRes.IsSuccessStatusCode)
            {
                var json = await enrolledRes.Content.ReadAsStringAsync();
                enrolled = JsonConvert.DeserializeObject<List<StudentCourseVM>>(json) ?? new();
            }

            var courseCards = new List<CourseCardVM>();
            foreach (var c in enrolled)
            {
                double attendanceRate = 0;
                double avg = 0;

                try
                {
                    var attRes = await _client.GetAsync($"{_apiBase}Attendance/user/{userId}/course/{c.CourseID}/percent");
                    if (attRes.IsSuccessStatusCode)
                        attendanceRate = double.Parse(await attRes.Content.ReadAsStringAsync());
                }
                catch { }

                try
                {
                    var avgRes = await _client.GetAsync($"{_apiBase}TestGrades/user/{userId}/course/{c.CourseID}/average");
                    if (avgRes.IsSuccessStatusCode)
                        avg = double.Parse(await avgRes.Content.ReadAsStringAsync());
                }
                catch { }

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

            // ---------- 2) Filters ----------
            var allDepartments = courseCards.Select(x => x.Department).Distinct().ToList();
            var allSemesters = courseCards.Select(x => x.Semester).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(department))
                courseCards = courseCards.Where(c => c.Department == department).ToList();
            if (!string.IsNullOrWhiteSpace(semester))
                courseCards = courseCards.Where(c => c.Semester == semester).ToList();

            // ---------- 3) GPA + Attendance ----------
            double gpa = 0;
            double overallAttendance = 0;
            try
            {
                var gpaRes = await _client.GetAsync($"{_apiBase}TestGrades/user/{userId}/gpa");
                if (gpaRes.IsSuccessStatusCode)
                    gpa = double.Parse(await gpaRes.Content.ReadAsStringAsync());
            }
            catch { }

            try
            {
                var attRes = await _client.GetAsync($"{_apiBase}Attendance/user/{userId}/percent");
                if (attRes.IsSuccessStatusCode)
                    overallAttendance = double.Parse(await attRes.Content.ReadAsStringAsync());
            }
            catch { }

            // ---------- 4) Temporary notifications ----------
            var notifications = new List<NotificationVM>
            {
                new NotificationVM { Message = "Welcome to your dashboard!", Type = "Info", CreatedAt = DateTime.Now },
                new NotificationVM { Message = $"You are enrolled in {courseCards.Count} courses.", Type = "Success", CreatedAt = DateTime.Now }
            };

            // ---------- 5) Mock chart data ----------
            var gradeSeries = new List<SeriesPointVM>
            {
                new SeriesPointVM { X = DateTime.Now.AddDays(-10), Y = 75 },
                new SeriesPointVM { X = DateTime.Now.AddDays(-5),  Y = 80 },
                new SeriesPointVM { X = DateTime.Now,             Y = 85 }
            };

            var attendanceSeries = new List<SeriesPointVM>
            {
                new SeriesPointVM { X = DateTime.Now.AddDays(-10), Y = 90 },
                new SeriesPointVM { X = DateTime.Now.AddDays(-5),  Y = 88 },
                new SeriesPointVM { X = DateTime.Now,              Y = 92 }
            };

            // ---------- 6) Assemble full dashboard ----------
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
