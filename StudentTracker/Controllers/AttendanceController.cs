using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AttendanceController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!.TrimEnd('/') + "/";
        }

        public async Task<IActionResult> Index(int courseId, int? sessionId)
        {
            ViewBag.CourseID = courseId;
            ViewBag.SelectedSessionId = sessionId;

            ViewBag.CourseName = await GetCourseName(courseId);

            var (semStart, semEnd) = await GetCurrentSemesterRangeOrFall2025();
            ViewBag.SemesterStart = semStart;
            ViewBag.SemesterEnd = semEnd;

            var sessions = await GetSessionsByCourse(courseId);

            sessions = sessions
                .Where(s => s.SessionDate.Date >= semStart.Date && s.SessionDate.Date <= semEnd.Date)
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .ToList();

            var sessionIdsWithAttendance = await GetSessionIdsWithAttendanceByCourse(courseId, semStart, semEnd);

            var sessionsWithAttendance = sessions
                .Where(s => sessionIdsWithAttendance.Contains(s.SessionID))
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .ToList();

            ViewBag.Sessions = sessionsWithAttendance;

            if (sessionId == null)
                return View(new List<AttendanceView>());

            if (!sessionsWithAttendance.Any(x => x.SessionID == sessionId.Value))
                return View(new List<AttendanceView>());

            var attendanceRes = await _client.GetAsync($"{_apiBase}Attendance/bySession/{sessionId.Value}");
            var attendance = new List<AttendanceView>();

            if (attendanceRes.IsSuccessStatusCode)
            {
                var json = await attendanceRes.Content.ReadAsStringAsync();
                attendance = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();
            }

            return View(attendance);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId, int? sessionId)
        {
            ViewBag.CourseID = courseId;
            ViewBag.SelectedSessionId = sessionId;

            ViewBag.CourseName = await GetCourseName(courseId);

            var (semStart, semEnd) = await GetCurrentSemesterRangeOrFall2025();
            ViewBag.SemesterStart = semStart;
            ViewBag.SemesterEnd = semEnd;

            var sessions = await GetSessionsByCourse(courseId);

            sessions = sessions
                .Where(s => s.SessionDate.Date >= semStart.Date && s.SessionDate.Date <= semEnd.Date)
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .ToList();

            ViewBag.Sessions = sessions;

            if (sessionId == null)
                return View(new List<AttendanceView>());

            var enrolled = await GetEnrolledStudentsByCourse(courseId);

            var rows = enrolled.Select(s => new AttendanceView
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                CourseID = courseId,
                SessionID = sessionId.Value,
                Status = "Present",
                IsPresent = true
            }).ToList();

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, int sessionId, List<AttendanceView> attendanceList)
        {
            if (courseId <= 0 || sessionId <= 0 || attendanceList == null || !attendanceList.Any())
                return RedirectToAction(nameof(Create), new { courseId });

            var errors = new List<string>();

            foreach (var record in attendanceList)
            {
                var apiAttendance = new
                {
                    StudentID = record.StudentID,
                    SessionID = sessionId,
                    CourseID = courseId,
                    IsPresent = record.Status == "Present"
                };

                var payload = JsonConvert.SerializeObject(apiAttendance);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{_apiBase}Attendance", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    errors.Add($"StudentID {record.StudentID}: {errorBody}");
                }
            }

            if (errors.Any())
            {
                TempData["AttendanceErrors"] = string.Join(" | ", errors);
                return RedirectToAction(nameof(Create), new { courseId, sessionId });
            }

            return RedirectToAction(nameof(Index), new { courseId, sessionId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int courseId, int sessionId)
        {
            await _client.DeleteAsync($"{_apiBase}Attendance/{id}");
            return RedirectToAction(nameof(Index), new { courseId, sessionId });
        }

        private async Task<List<CourseSessionView>> GetSessionsByCourse(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSessions/course/{courseId}");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CourseSessionView>>(json) ?? new();
        }

        private async Task<HashSet<int>> GetSessionIdsWithAttendanceByCourse(int courseId, DateTime startDate, DateTime endDate)
        {
            var url = $"{_apiBase}Attendance/sessionIdsByCourse/{courseId}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var res = await _client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new HashSet<int>();

            var json = await res.Content.ReadAsStringAsync();
            var ids = JsonConvert.DeserializeObject<List<int>>(json) ?? new List<int>();
            return ids.ToHashSet();
        }

        private async Task<List<StudentMiniView>> GetEnrolledStudentsByCourse(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<StudentMiniView>>(json) ?? new();
        }

        private async Task<(DateTime start, DateTime end)> GetCurrentSemesterRangeOrFall2025()
        {
            try
            {
                var res = await _client.GetAsync($"{_apiBase}Semesters");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var semesters = JsonConvert.DeserializeObject<List<SemesterMiniView>>(json) ?? new();
                    var today = DateTime.Today;

                    var current = semesters.FirstOrDefault(s => today.Date >= s.StartDate.Date && today.Date <= s.EndDate.Date);
                    if (current != null)
                        return (current.StartDate.Date, current.EndDate.Date);
                }
            }
            catch { }

            return (new DateTime(2025, 9, 1), new DateTime(2025, 12, 20));
        }

        private async Task<string> GetCourseName(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!res.IsSuccessStatusCode) return "Course";

            var json = await res.Content.ReadAsStringAsync();

            try
            {
                var dto = JsonConvert.DeserializeObject<CourseNameDto>(json);
                if (!string.IsNullOrWhiteSpace(dto?.CourseName)) return dto.CourseName!;
                if (!string.IsNullOrWhiteSpace(dto?.Name)) return dto.Name!;
                if (!string.IsNullOrWhiteSpace(dto?.Title)) return dto.Title!;
            }
            catch { }

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict != null)
                {
                    foreach (var key in new[] { "courseName", "CourseName", "name", "Name", "title", "Title" })
                    {
                        if (dict.TryGetValue(key, out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString()))
                            return v.ToString()!;
                    }
                }
            }
            catch { }

            return "Course";
        }

        private class StudentMiniView
        {
            public int StudentID { get; set; }
            public string StudentName { get; set; } = "";
        }

        private class SemesterMiniView
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        private class CourseNameDto
        {
            public string? CourseName { get; set; }
            public string? Name { get; set; }
            public string? Title { get; set; }
        }
    }
}
