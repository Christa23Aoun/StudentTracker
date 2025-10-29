using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class TeacherController : Controller
    {
        private readonly HttpClient _http;

        public TeacherController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1. Get teacher info from session
            var teacherId = HttpContext.Session.GetInt32("UserID");
            var teacherName = HttpContext.Session.GetString("UserName");
            var teacherEmail = HttpContext.Session.GetString("UserEmail");

            ViewBag.TeacherName = teacherName ?? "Unknown Teacher";
            ViewBag.TeacherEmail = teacherEmail ?? "unknown@mail.com";

            if (teacherId == null)
            {
                // If no session, redirect to login
                return RedirectToAction("LoginTeacher", "Auth");
            }

            //  2. Fetch teacher's courses from API
            // 2) Fetch teacher's courses from API (deserialize CourseListItem)
            var courses = new List<TeacherCourseRowDto>();

            try
            {
                var response = await _http.GetAsync($"api/Courses/byTeacher/{teacherId}");
                Console.WriteLine($"[DEBUG] API call: api/Courses/byTeacher/{teacherId} -> {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize to CourseListItem (matches your JSON)
                    var dbCourses = await response.Content
                        .ReadFromJsonAsync<List<StudentTrackerCOMMON.Models.CourseListItem>>();
                    Console.WriteLine($"[DEBUG] Received {dbCourses?.Count} courses from API");

                    if (dbCourses != null)
                    {
                        // Map to the rows the view expects
                        foreach (var c in dbCourses)
                        {
                            courses.Add(new TeacherCourseRowDto
                            {
                                CourseID = c.CourseID,
                                CourseCode = c.CourseCode,
                                CourseName = c.CourseName,
                                DepartmentName = c.DepartmentName ?? "",
                                SemesterName = c.SemesterName ?? "",
                                // If you later expose real metrics from API, bind them here.
                                StudentCount = 0,
                                AverageGrade = 0,
                                AttendanceRate = 0
                            });
                        }
                    }
                    else
                    {
                        ViewBag.Error = "No courses found (empty response).";
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

            //  3. Build the TeacherDashboardDto model
            var dashboard = new TeacherDashboardDto
            {
                CourseCount = courses?.Count ?? 0,
                StudentCount = courses?.Sum(c => c.StudentCount) ?? 0,
                AverageGrade = (courses != null && courses.Any()) ? (int)courses.Average(c => c.AverageGrade) : 0,
                AttendanceRate = (courses != null && courses.Any()) ? (int)courses.Average(c => c.AttendanceRate) : 0,
                Courses = courses ?? new List<TeacherCourseRowDto>(),
                RecentActivities = new List<RecentActivityDto>
                {
                    new() { Description = "Added new test" },
                    new() { Description = "Updated grades" },
                    new() { Description = "Recorded attendance" }
                }
            };

            //  4. Return dashboard object to the view
            return View(dashboard);
        }
    }
}
