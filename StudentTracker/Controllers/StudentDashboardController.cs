using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using StudentTracker.Models;

namespace StudentTracker.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly HttpClient _api;

        public StudentDashboardController(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("API");
        }

        // ✅ Default dashboard page
        public async Task<IActionResult> Index(int userId = 2, string? semester = null, string? department = null)
        {
            // --- 1) Fetch enrolled courses ---
            var enrolled = await _api.GetFromJsonAsync<List<StudentCourseVM>>($"api/StudentCourses/user/{userId}")
                ?? new List<StudentCourseVM>();



            var courseCards = new List<CourseCardVM>();
            foreach (var c in enrolled)
            {
                int courseId = c.CourseID;
                string courseName = c.CourseName;
                string teacherName = c.TeacherName;
                string dept = c.Department;
                string sem = c.Semester;

                double attendanceRate = 0;
                try { attendanceRate = await _api.GetFromJsonAsync<double>($"api/Attendance/user/{userId}/course/{courseId}/percent"); } catch { }

                double avg = 0;
                try { avg = await _api.GetFromJsonAsync<double>($"api/TestGrades/user/{userId}/course/{courseId}/average"); } catch { }

                courseCards.Add(new CourseCardVM
                {
                    CourseID = courseId,
                    CourseName = courseName,
                    TeacherName = teacherName,
                    Department = dept,
                    Semester = sem,
                    AttendanceRate = attendanceRate,
                    CurrentAverage = avg
                });
            }



            // --- 2) Filters ---
            var allDepartments = courseCards.Select(x => x.Department).Distinct().ToList();
            var allSemesters = courseCards.Select(x => x.Semester).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(department))
                courseCards = courseCards.Where(c => c.Department == department).ToList();
            if (!string.IsNullOrWhiteSpace(semester))
                courseCards = courseCards.Where(c => c.Semester == semester).ToList();

            // --- 3) GPA + Attendance ---
            double gpa = 0;
            try { gpa = await _api.GetFromJsonAsync<double>($"api/TestGrades/user/{userId}/gpa"); } catch { }
            double overallAttendance = 0;
            try { overallAttendance = await _api.GetFromJsonAsync<double>($"api/Attendance/user/{userId}/percent"); } catch { }

            // --- 4) Mock notifications (since Notifications API not ready) ---
            var notifications = new List<NotificationVM>
            {
                new NotificationVM { Message = "Welcome to your dashboard!", Type = "Info", CreatedAt = DateTime.Now },
                new NotificationVM { Message = "You are enrolled in 2 courses.", Type = "Success", CreatedAt = DateTime.Now }
            };

            // --- 5) Mock charts ---
            var gradeSeries = new List<SeriesPointVM>
            {
                new SeriesPointVM { X = DateTime.Now.AddDays(-10), Y = 75 },
                new SeriesPointVM { X = DateTime.Now.AddDays(-5), Y = 80 },
                new SeriesPointVM { X = DateTime.Now, Y = 85 }
            };
            var attendanceSeries = new List<SeriesPointVM>
            {
                new SeriesPointVM { X = DateTime.Now.AddDays(-10), Y = 90 },
                new SeriesPointVM { X = DateTime.Now.AddDays(-5), Y = 88 },
                new SeriesPointVM { X = DateTime.Now, Y = 92 }
            };

            // --- 6) Assemble ViewModel ---
            var vm = new StudentDashboardVM
            {
                UserID = userId,
                StudentName = "Student",
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
