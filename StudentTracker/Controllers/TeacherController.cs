using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;
using System.Security.Claims;

namespace StudentTracker.Controllers
{
    [Authorize] // keep auth, but cookie must be set at login (we added that earlier)
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

            var courses = new List<TeacherCourseRowView>();

            try
            {
                // 1) get courses assigned to teacher
                var response = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Failed to load courses ({response.StatusCode}).";
                }
                else
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiCourses = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

                    // For each course, fetch stats in parallel
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

                        // Build endpoint URIs (adapt if your API differs)
                        var studentCountUri = $"{_apiBase}StudentCourse/countByCourse/{c.CourseID}";
                        var averageUri = $"{_apiBase}TestGrades/averageByCourse/{c.CourseID}";
                        var attendanceUri = $"{_apiBase}Attendance/rateByCourse/{c.CourseID}";

                        // Fire requests in parallel and tolerate failures
                        var tCount = _client.GetAsync(studentCountUri);
                        var tAvg = _client.GetAsync(averageUri);
                        var tAtt = _client.GetAsync(attendanceUri);

                        await Task.WhenAll(tCount, tAvg, tAtt);

                        try
                        {
                            if (tCount.Result.IsSuccessStatusCode)
                            {
                                var cJson = await tCount.Result.Content.ReadAsStringAsync();
                                if (int.TryParse(cJson, out var cnt))
                                    row.StudentCount = cnt;
                                else
                                {
                                    // if API returns object: try deserialize
                                    try
                                    {
                                        var tmp = JsonConvert.DeserializeObject<dynamic>(cJson);
                                        row.StudentCount = (int?)tmp?.count ?? row.StudentCount;
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { /* ignore and keep 0 */ }

                        try
                        {
                            if (tAvg.Result.IsSuccessStatusCode)
                            {
                                var aJson = await tAvg.Result.Content.ReadAsStringAsync();
                                if (double.TryParse(aJson, out var avg))
                                    row.AverageGrade = avg;
                                else
                                {
                                    try
                                    {
                                        var tmp = JsonConvert.DeserializeObject<dynamic>(aJson);
                                        row.AverageGrade = (double?)tmp?.average ?? row.AverageGrade;
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }

                        try
                        {
                            if (tAtt.Result.IsSuccessStatusCode)
                            {
                                var attJson = await tAtt.Result.Content.ReadAsStringAsync();
                                if (double.TryParse(attJson, out var rate))
                                    row.AttendanceRate = rate;
                                else
                                {
                                    try
                                    {
                                        var tmp = JsonConvert.DeserializeObject<dynamic>(attJson);
                                        row.AttendanceRate = (double?)tmp?.rate ?? row.AttendanceRate;
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }

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

            // Build dashboard view model with real numbers
            var dashboard = new TeacherDashboardView
            {
                TeacherName = teacherName ?? "Teacher",
                TeacherEmail = teacherEmail ?? string.Empty,
                TotalCourses = courses.Count,
                TotalStudents = courses.Sum(c => c.StudentCount),
                AverageGrade = courses.Any() ? Math.Round(courses.Average(c => c.AverageGrade), 2) : 0,
                AttendanceRate = courses.Any() ? Math.Round(courses.Average(c => c.AttendanceRate), 2) : 0,
                Courses = courses.ToList(),
                RecentActivities = new List<RecentActivityView>() // you can load real activities similarly
            };

            return View("~/Views/Teacher/Dashboard.cshtml", dashboard);
        }
    }
}
