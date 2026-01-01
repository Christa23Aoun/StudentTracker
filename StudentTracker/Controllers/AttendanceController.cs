using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<IActionResult> Index(int courseId, int? sessionId, int? semesterId)
        {
            ViewBag.CourseID = courseId;
            ViewBag.SelectedSessionId = sessionId;
            ViewBag.CourseName = await GetCourseName(courseId);

            var semesters = await GetSemesters();
            ViewBag.Semesters = semesters;

            var selectedSemester = await ResolveSelectedSemester(courseId, semesters, semesterId);
            ViewBag.SelectedSemesterId = selectedSemester?.SemesterID;

            var sessions = await GetSessionsByCourse(courseId);

            if (selectedSemester != null)
            {
                sessions = sessions
                    .Where(s =>
                        s.SessionDate.Date >= selectedSemester.StartDate.Date &&
                        s.SessionDate.Date <= selectedSemester.EndDate.Date)
                    .ToList();
            }

            var usedSessionIds = selectedSemester != null
                ? await GetSessionIdsWithAttendanceByCourse(
                    courseId,
                    selectedSemester.StartDate,
                    selectedSemester.EndDate)
                : new HashSet<int>();

            sessions = sessions
                .Where(s => usedSessionIds.Contains(s.SessionID))
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .ToList();

            ViewBag.Sessions = sessions;

            if (sessionId == null || !sessions.Any(s => s.SessionID == sessionId))
                return View(new List<AttendanceView>());

            var attendanceRes =
                await _client.GetAsync($"{_apiBase}Attendance/bySession/{sessionId.Value}");

            if (!attendanceRes.IsSuccessStatusCode)
                return View(new List<AttendanceView>());

            var json = await attendanceRes.Content.ReadAsStringAsync();
            return View(JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId, int? sessionId, int? semesterId)
        {
            ViewBag.CourseID = courseId;
            ViewBag.SelectedSessionId = sessionId;
            ViewBag.CourseName = await GetCourseName(courseId);

            var semesters = await GetSemesters();
            ViewBag.Semesters = semesters;

            var selectedSemester = await ResolveSelectedSemester(courseId, semesters, semesterId);
            ViewBag.SelectedSemesterId = selectedSemester?.SemesterID;

            var sessions = await GetSessionsByCourse(courseId);

            if (selectedSemester != null)
            {
                sessions = sessions
                    .Where(s =>
                        s.SessionDate.Date >= selectedSemester.StartDate.Date &&
                        s.SessionDate.Date <= selectedSemester.EndDate.Date)
                    .OrderByDescending(s => s.SessionDate)
                    .ThenByDescending(s => s.StartTime)
                    .ToList();
            }
            else
            {
                sessions = sessions
                    .OrderByDescending(s => s.SessionDate)
                    .ThenByDescending(s => s.StartTime)
                    .ToList();
            }

            var usedSessionIds = new HashSet<int>();

            if (selectedSemester != null)
            {
                usedSessionIds = await GetSessionIdsWithAttendanceByCourse(
                    courseId,
                    selectedSemester.StartDate,
                    selectedSemester.EndDate);
            }
            else
            {
                usedSessionIds = await GetSessionIdsWithAttendanceByCourse(
                    courseId,
                    DateTime.MinValue.Date,
                    DateTime.MaxValue.Date);
            }

            var availableSessions = sessions
                .Where(s => !usedSessionIds.Contains(s.SessionID))
                .ToList();

            ViewBag.Sessions = availableSessions;

            if (sessionId == null || !availableSessions.Any(s => s.SessionID == sessionId))
                return View(new List<AttendanceView>());

            var enrolled = await GetEnrolledStudentsByCourse(courseId);

            return View(enrolled.Select(s => new AttendanceView
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                CourseID = courseId,
                SessionID = sessionId.Value,
                Status = "Present",
                IsPresent = true
            }).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int courseId,
            int sessionId,
            int? semesterId,
            List<AttendanceView> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
                return RedirectToAction(nameof(Create),
                    new { courseId, sessionId, semesterId });

            foreach (var record in attendanceList)
            {
                var payload = JsonConvert.SerializeObject(new
                {
                    StudentID = record.StudentID,
                    SessionID = sessionId,
                    CourseID = courseId,
                    IsPresent = record.Status == "Present"
                });

                await _client.PostAsync(
                    $"{_apiBase}Attendance",
                    new StringContent(payload, Encoding.UTF8, "application/json"));
            }

            return RedirectToAction(nameof(Index),
                new { courseId, sessionId, semesterId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int courseId,
            int sessionId,
            int? semesterId)
        {
            await _client.DeleteAsync($"{_apiBase}Attendance/{id}");

            return RedirectToAction(nameof(Index),
                new { courseId, sessionId, semesterId });
        }

        private async Task<List<CourseSessionView>> GetSessionsByCourse(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSessions/course/{courseId}");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CourseSessionView>>(json) ?? new();
        }

        private async Task<List<SemesterOptionView>> GetSemesters()
        {
            var res = await _client.GetAsync($"{_apiBase}Lookups/semesters");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();

            try
            {
                var arr = JArray.Parse(json);
                var list = new List<SemesterOptionView>();

                foreach (var t in arr)
                {
                    var idToken =
                        t["SemesterID"] ??
                        t["semesterID"] ??
                        t["semesterId"] ??
                        t["id"] ??
                        t["ID"];

                    if (idToken == null) continue;
                    if (!int.TryParse(idToken.ToString(), out var id)) continue;
                    if (id <= 0) continue;

                    var nameToken =
                        t["Name"] ??
                        t["name"] ??
                        t["SemesterName"] ??
                        t["semesterName"] ??
                        t["TermName"] ??
                        t["termName"] ??
                        t["Title"] ??
                        t["title"];

                    var startToken =
                        t["StartDate"] ??
                        t["startDate"] ??
                        t["SemesterStartDate"] ??
                        t["semesterStartDate"] ??
                        t["Start"] ??
                        t["start"];

                    var endToken =
                        t["EndDate"] ??
                        t["endDate"] ??
                        t["SemesterEndDate"] ??
                        t["semesterEndDate"] ??
                        t["End"] ??
                        t["end"];

                    DateTime start = DateTime.MinValue;
                    DateTime end = DateTime.MaxValue;

                    if (startToken != null && DateTime.TryParse(startToken.ToString(), out var sd))
                        start = sd;

                    if (endToken != null && DateTime.TryParse(endToken.ToString(), out var ed))
                        end = ed;

                    if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                        continue;

                    list.Add(new SemesterOptionView
                    {
                        SemesterID = id,
                        Name = nameToken?.ToString() ?? $"Semester {id}",
                        StartDate = start,
                        EndDate = end
                    });
                }

                return list.OrderByDescending(s => s.StartDate).ToList();
            }
            catch
            {
                var list = JsonConvert.DeserializeObject<List<SemesterOptionView>>(json) ?? new();
                return list
                    .Where(s => s.SemesterID > 0 && s.EndDate >= s.StartDate)
                    .OrderByDescending(s => s.StartDate)
                    .ToList();
            }
        }

        private async Task<HashSet<int>> GetSessionIdsWithAttendanceByCourse(
            int courseId, DateTime startDate, DateTime endDate)
        {
            var res = await _client.GetAsync(
                $"{_apiBase}Attendance/sessionIdsByCourse/{courseId}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<int>>(json)?.ToHashSet() ?? new();
        }

        private async Task<List<StudentMiniView>> GetEnrolledStudentsByCourse(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<StudentMiniView>>(json) ?? new();
        }

        private async Task<string> GetCourseName(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!res.IsSuccessStatusCode) return "";

            var json = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            var nameToken =
                obj["CourseName"] ??
                obj["courseName"] ??
                obj["name"];

            return nameToken?.ToString() ?? "";
        }

        private async Task<int?> GetCourseSemesterId(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            var token =
                obj["SemesterID"] ??
                obj["semesterID"] ??
                obj["semesterId"];

            if (token == null) return null;

            if (int.TryParse(token.ToString(), out var id) && id > 0)
                return id;

            return null;
        }

        private async Task<SemesterOptionView?> ResolveSelectedSemester(
            int courseId,
            List<SemesterOptionView> semesters,
            int? semesterId)
        {
            if (semesters == null || semesters.Count == 0)
                return null;

            if (semesterId.HasValue && semesterId.Value > 0)
            {
                var byId = semesters.FirstOrDefault(s => s.SemesterID == semesterId.Value);
                if (byId != null) return byId;
            }

            var courseSemesterId = await GetCourseSemesterId(courseId);
            if (courseSemesterId.HasValue)
            {
                var byCourse = semesters.FirstOrDefault(s => s.SemesterID == courseSemesterId.Value);
                if (byCourse != null) return byCourse;
            }

            var today = DateTime.Today;
            var byToday = semesters.FirstOrDefault(s => today.Date >= s.StartDate.Date && today.Date <= s.EndDate.Date);
            if (byToday != null) return byToday;

            return semesters.OrderByDescending(s => s.StartDate).FirstOrDefault();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int courseId, int sessionId, int? semesterId)
        {
            var res = await _client.GetAsync($"{_apiBase}Attendance/bySession/{sessionId}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId, sessionId, semesterId });

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();

            var attendance = list.FirstOrDefault(a => a.AttendanceID == id);
            if (attendance == null)
                return RedirectToAction(nameof(Index), new { courseId, sessionId, semesterId });

            ViewBag.CourseID = courseId;
            ViewBag.SessionID = sessionId;
            ViewBag.SemesterID = semesterId;

            return View(attendance);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AttendanceView model,
            int courseId,
            int sessionId,
            int? semesterId)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                AttendanceID = model.AttendanceID,
                CourseID = courseId,
                IsPresent = model.IsPresent
            });

            await _client.PutAsync(
                $"{_apiBase}Attendance",
                new StringContent(payload, Encoding.UTF8, "application/json"));

            return RedirectToAction(nameof(Index), new { courseId, sessionId, semesterId });
        }

        private class StudentMiniView
        {
            public int StudentID { get; set; }
            public string StudentName { get; set; } = "";
        }

        public class SemesterOptionView
        {
            public int SemesterID { get; set; }
            public string Name { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
}
