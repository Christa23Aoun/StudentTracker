//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using StudentTracker.Models;
//using System.Text;
//using System.Security.Claims;

//namespace StudentTracker.Controllers
//{
//    [Authorize] 
//    public class TeacherController : Controller
//    {
//        private readonly HttpClient _client;
//        private readonly string _apiBase;

//        public TeacherController(IHttpClientFactory factory, IConfiguration config)
//        {
//            _client = factory.CreateClient();
//            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
//        }

//        public async Task<IActionResult> Dashboard()
//        {
//            var teacherId = HttpContext.Session.GetInt32("UserID");
//            var teacherName = HttpContext.Session.GetString("UserName");
//            var teacherEmail = HttpContext.Session.GetString("UserEmail");

//            if (teacherId == null)
//            {
//                TempData["Error"] = "Please log in first.";
//                return RedirectToAction("LoginTeacher", "Auth");
//            }

//            ViewBag.TeacherName = teacherName ?? "Unknown Teacher";
//            ViewBag.TeacherEmail = teacherEmail ?? "unknown@mail.com";

//            var courses = new List<TeacherCourseRowView>();

//            try
//            {
//                var response = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
//                if (!response.IsSuccessStatusCode)
//                {
//                    ViewBag.Error = $"Failed to load courses ({response.StatusCode}).";
//                }
//                else
//                {
//                    var json = await response.Content.ReadAsStringAsync();
//                    var apiCourses = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

//                    var courseTasks = apiCourses.Select(async c =>
//                    {
//                        var row = new TeacherCourseRowView
//                        {
//                            CourseID = c.CourseID,
//                            CourseCode = c.CourseCode,
//                            CourseName = c.CourseName,
//                            DepartmentName = c.DepartmentName ?? string.Empty,
//                            SemesterName = c.SemesterName ?? string.Empty,
//                            StudentCount = 0,
//                            AverageGrade = 0,
//                            AttendanceRate = 0
//                        };

//                        var studentCountUri = $"{_apiBase}StudentCourse/countByCourse/{c.CourseID}";
//                        var averageUri = $"{_apiBase}TestGrades/averageByCourse/{c.CourseID}";
//                        var attendanceUri = $"{_apiBase}Attendance/rateByCourse/{c.CourseID}";

//                        var tCount = _client.GetAsync(studentCountUri);
//                        var tAvg = _client.GetAsync(averageUri);
//                        var tAtt = _client.GetAsync(attendanceUri);

//                        await Task.WhenAll(tCount, tAvg, tAtt);

//                        try
//                        {
//                            if (tCount.Result.IsSuccessStatusCode)
//                            {
//                                var cJson = await tCount.Result.Content.ReadAsStringAsync();
//                                if (int.TryParse(cJson, out var cnt))
//                                    row.StudentCount = cnt;
//                                else
//                                {
//                                    try
//                                    {
//                                        var tmp = JsonConvert.DeserializeObject<dynamic>(cJson);
//                                        row.StudentCount = (int?)tmp?.count ?? row.StudentCount;
//                                    }
//                                    catch { }
//                                }
//                            }
//                        }
//                        catch { /* ignore and keep 0 */ }

//                        try
//                        {
//                            if (tAvg.Result.IsSuccessStatusCode)
//                            {
//                                var aJson = await tAvg.Result.Content.ReadAsStringAsync();
//                                if (double.TryParse(aJson, out var avg))
//                                    row.AverageGrade = avg;
//                                else
//                                {
//                                    try
//                                    {
//                                        var tmp = JsonConvert.DeserializeObject<dynamic>(aJson);
//                                        row.AverageGrade = (double?)tmp?.average ?? row.AverageGrade;
//                                    }
//                                    catch { }
//                                }
//                            }
//                        }
//                        catch { }

//                        try
//                        {
//                            if (tAtt.Result.IsSuccessStatusCode)
//                            {
//                                var attJson = await tAtt.Result.Content.ReadAsStringAsync();
//                                if (double.TryParse(attJson, out var rate))
//                                    row.AttendanceRate = rate;
//                                else
//                                {
//                                    try
//                                    {
//                                        var tmp = JsonConvert.DeserializeObject<dynamic>(attJson);
//                                        row.AttendanceRate = (double?)tmp?.rate ?? row.AttendanceRate;
//                                    }
//                                    catch { }
//                                }
//                            }
//                        }
//                        catch { }

//                        return row;
//                    }).ToList();

//                    var resolved = await Task.WhenAll(courseTasks);
//                    courses.AddRange(resolved);
//                }
//            }
//            catch (Exception ex)
//            {
//                ViewBag.Error = "Could not load courses from API: " + ex.Message;
//            }

//            // Build dashboard view model with real numbers
//            var dashboard = new TeacherDashboardView
//            {
//                TeacherName = teacherName ?? "Teacher",
//                TeacherEmail = teacherEmail ?? string.Empty,
//                TotalCourses = courses.Count,
//                TotalStudents = courses.Sum(c => c.StudentCount),
//                AverageGrade = courses.Any() ? Math.Round(courses.Average(c => c.AverageGrade), 2) : 0,
//                AttendanceRate = courses.Any() ? Math.Round(courses.Average(c => c.AttendanceRate), 2) : 0,
//                Courses = courses.ToList(),
//                RecentActivities = new List<RecentActivityView>() 
//            };

//            return View("~/Views/Teacher/Dashboard.cshtml", dashboard);
//        }
//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Security.Claims;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TeacherController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Dashboard()
        {
            var teacherId = HttpContext.Session.GetInt32("TeacherID") ?? HttpContext.Session.GetInt32("UserID");
            var teacherName = HttpContext.Session.GetString("UserName");
            var teacherEmail = HttpContext.Session.GetString("UserEmail");

            // 🔍 Debug lines to confirm actual values during runtime
            Console.WriteLine($"🎯 TeacherID in session: {HttpContext.Session.GetInt32("TeacherID")}");
            Console.WriteLine($"🎯 UserID in session: {HttpContext.Session.GetInt32("UserID")}");

            if (teacherId == null)
            {
                TempData["Error"] = "Please log in first.";
                return RedirectToAction("LoginTeacher", "Auth");
            }

            ViewBag.TeacherName = teacherName ?? "Unknown Teacher";
            ViewBag.TeacherEmail = teacherEmail ?? "unknown@mail.com";

            var courses = new List<TeacherCourseRowView>();

            try
            {
                var response = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiCourses = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

                    var courseTasks = apiCourses.Select(async c =>
                    {
                        var row = new TeacherCourseRowView
                        {
                            CourseID = c.CourseID,
                            CourseCode = c.CourseCode,
                            CourseName = c.CourseName,
                            DepartmentName = c.DepartmentName ?? string.Empty,
                            SemesterName = c.SemesterName ?? string.Empty,
                            StudentCount = 0,
                            AverageGrade = 0,
                            AttendanceRate = 0
                        };

                        var studentCountUri = $"{_apiBase}StudentCourse/countByCourse/{c.CourseID}";
                        var averageUri = $"{_apiBase}TestGrades/averageByCourse/{c.CourseID}";
                        var attendanceUri = $"{_apiBase}Attendance/rateByCourse/{c.CourseID}";

                        var tCount = _client.GetAsync(studentCountUri);
                        var tAvg = _client.GetAsync(averageUri);
                        var tAtt = _client.GetAsync(attendanceUri);

                        await Task.WhenAll(tCount, tAvg, tAtt);

                        if (tCount.Result.IsSuccessStatusCode)
                        {
                            var cJson = await tCount.Result.Content.ReadAsStringAsync();
                            if (int.TryParse(cJson, out var sc))
                                row.StudentCount = sc;
                        }

                        if (tAvg.Result.IsSuccessStatusCode)
                        {
                            var aJson = await tAvg.Result.Content.ReadAsStringAsync();
                            if (double.TryParse(aJson, out var avg))
                                row.AverageGrade = avg;
                        }

                        if (tAtt.Result.IsSuccessStatusCode)
                        {
                            var attJson = await tAtt.Result.Content.ReadAsStringAsync();
                            if (double.TryParse(attJson, out var att))
                                row.AttendanceRate = att;
                        }

                        return row;
                    }).ToList();

                    var resolved = await Task.WhenAll(courseTasks);
                    courses.AddRange(resolved);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not load courses from API: " + ex.Message;
            }

            var dashboard = new TeacherDashboardView
            {
                TeacherName = teacherName ?? "Teacher",
                TeacherEmail = teacherEmail ?? string.Empty,
                TotalCourses = courses.Count,
                TotalStudents = courses.Sum(c => c.StudentCount),
                AverageGrade = courses.Any() ? Math.Round(courses.Average(c => c.AverageGrade), 2) : 0,
                AttendanceRate = courses.Any() ? Math.Round(courses.Average(c => c.AttendanceRate), 2) : 0,
                Courses = courses.ToList(),
                RecentActivities = new List<RecentActivityView>()
            };

            return View("~/Views/Teacher/Dashboard.cshtml", dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            var response = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(json);
            return View("~/Views/Teacher/CourseDetails.cshtml", course);
        }
    }
}
