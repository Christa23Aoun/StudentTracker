using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models.ViewModels;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class StudentDashboardController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentDashboardController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value ?? "https://localhost:7199/";
        }

        // GET: /StudentDashboard
        public async Task<IActionResult> Index(int? studentId, string? semester, string? department)
        {
            // If you store StudentId in session/claims, you can pull it from there when null.
            var id = studentId ?? HttpContext.Session.GetInt32("StudentId") ?? 1;

            StudentDashboardVM model;
            try
            {
                // Primary API (single payload) — preferred if your API exposes it
                var res = await _client.GetAsync($"{_apiBase}api/Dashboard/Student/{id}?semester={semester}&department={department}");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<StudentDashboardVM>(json) ?? new StudentDashboardVM();
                }
                else
                {
                    // Fallback: compose from separate endpoints
                    model = await BuildFromSeparateEndpoints(id, semester, department);
                }
            }
            catch
            {
                // Safe demo data if API is down so the page still renders
                model = DemoData();
            }

            // Apply filters on the client side if needed
            if (!string.IsNullOrWhiteSpace(semester))
                model.MyCourses = model.MyCourses.Where(c => c.CourseName.Contains(semester, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(department))
                model.MyCourses = model.MyCourses.Where(c => c.Department.Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();

            return View(model);
        }

        private async Task<StudentDashboardVM> BuildFromSeparateEndpoints(int studentId, string? semester, string? department)
        {
            var model = new StudentDashboardVM();

            // Overview
            var overview = await _client.GetAsync($"{_apiBase}api/Students/{studentId}/overview");
            if (overview.IsSuccessStatusCode)
            {
                dynamic o = JsonConvert.DeserializeObject(await overview.Content.ReadAsStringAsync())!;
                model.StudentName = o.studentName;
                model.CurrentSemester = o.currentSemester;
                model.ActiveCourseCount = (int)o.activeCourseCount;
                model.GPA = (double)o.gpa;
                model.AttendancePercent = (double)o.attendancePercent;
                model.Semesters = ((IEnumerable<object>)o.semesters).Select(s => s.ToString()!).ToList();
                model.Departments = ((IEnumerable<object>)o.departments).Select(s => s.ToString()!).ToList();
            }

            // Courses
            var courses = await _client.GetAsync($"{_apiBase}api/Students/{studentId}/courses?semester={semester}&department={department}");
            if (courses.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<CourseItemVM>>(await courses.Content.ReadAsStringAsync());
                if (list != null) model.MyCourses = list;
            }

            // Notifications
            var notifs = await _client.GetAsync($"{_apiBase}api/Students/{studentId}/notifications?unreadOnly=false");
            if (notifs.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<NotificationVM>>(await notifs.Content.ReadAsStringAsync());
                if (list != null) model.Notifications = list.OrderByDescending(n => n.CreatedAt).ToList();
            }

            // Charts
            var grades = await _client.GetAsync($"{_apiBase}api/Students/{studentId}/grades/progress");
            if (grades.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<GradePointVM>>(await grades.Content.ReadAsStringAsync());
                if (list != null) model.GradeProgress = list;
            }
            var attendance = await _client.GetAsync($"{_apiBase}api/Students/{studentId}/attendance/trend");
            if (attendance.IsSuccessStatusCode)
            {
                var list = JsonConvert.DeserializeObject<List<AttendancePointVM>>(await attendance.Content.ReadAsStringAsync());
                if (list != null) model.AttendanceTrend = list;
            }

            return model;
        }

        private StudentDashboardVM DemoData()
        {
            return new StudentDashboardVM
            {
                StudentName = "Lynn El-Haly",
                CurrentSemester = "Fall 2025",
                ActiveCourseCount = 4,
                GPA = 3.42,
                AttendancePercent = 92.5,
                Semesters = new() { "Fall 2025", "Spring 2025", "Fall 2024" },
                Departments = new() { "CS", "Math", "Physics" },
                MyCourses = new()
                {
                    new CourseItemVM{ CourseId=1, CourseName="Operating Systems", TeacherName="Dr. Tannous", Department="CS", AttendanceRate=95, CurrentAverage=88},
                    new CourseItemVM{ CourseId=2, CourseName="Data Structures", TeacherName="Dr. Gerges", Department="CS", AttendanceRate=90, CurrentAverage=91},
                    new CourseItemVM{ CourseId=3, CourseName="Discrete Math", TeacherName="Dr. Hachem", Department="Math", AttendanceRate=89, CurrentAverage=84},
                    new CourseItemVM{ CourseId=4, CourseName="Physics I", TeacherName="Dr. Salem", Department="Physics", AttendanceRate=96, CurrentAverage=86},
                },
                Notifications = new()
                {
                    new NotificationVM{ NotificationId=101, CreatedAt=DateTime.UtcNow.AddDays(-1), Title="New grade posted", Message="OS Midterm grade is available.", Type="success", IsRead=false},
                    new NotificationVM{ NotificationId=102, CreatedAt=DateTime.UtcNow.AddDays(-3), Title="Attendance Warning", Message="Data Structures attendance at 70%.", Type="warning", IsRead=false},
                    new NotificationVM{ NotificationId=103, CreatedAt=DateTime.UtcNow.AddDays(-7), Title="Course update", Message="Discrete Math room changed to B302.", Type="info", IsRead=true},
                },
                GradeProgress = new()
                {
                    new GradePointVM{ Label="Quiz 1", Average=78},
                    new GradePointVM{ Label="Quiz 2", Average=82},
                    new GradePointVM{ Label="Midterm", Average=86},
                    new GradePointVM{ Label="Project", Average=90},
                },
                AttendanceTrend = new()
                {
                    new AttendancePointVM{ WeekLabel="Wk1", Percent=100},
                    new AttendancePointVM{ WeekLabel="Wk2", Percent=95},
                    new AttendancePointVM{ WeekLabel="Wk3", Percent=88},
                    new AttendancePointVM{ WeekLabel="Wk4", Percent=92},
                    new AttendancePointVM{ WeekLabel="Wk5", Percent=94},
                }
            };
        }
    }
}