using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
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
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            var teacherEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(teacherEmail))
                return RedirectToAction("Login", "Auth");

            var teacherRes = await _client.GetAsync($"{_apiBase}TeacherDashboard/byEmail/{teacherEmail}");
            if (!teacherRes.IsSuccessStatusCode)
                return RedirectToAction("Dashboard", "Teacher");

            var teacherJson = await teacherRes.Content.ReadAsStringAsync();
            var teacher = JsonConvert.DeserializeObject<TeacherDashboardView>(teacherJson);
            int teacherId = teacher?.TeacherID ?? 0;

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
            var courseList = new List<CourseView>();
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                courseList = JsonConvert.DeserializeObject<List<CourseView>>(cJson) ?? new();
            }

            var list = new List<AttendanceView>();
            if (courseId.HasValue)
            {
                var res = await _client.GetAsync($"{_apiBase}Attendance/byCourse/{courseId}");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    list = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();
                }
            }

            ViewBag.TeacherCourses = courseList;
            ViewBag.CourseID = courseId;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            var teacherEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(teacherEmail))
                return RedirectToAction("Login", "Auth");

            var teacherRes = await _client.GetAsync($"{_apiBase}TeacherDashboard/byEmail/{teacherEmail}");
            var teacherJson = await teacherRes.Content.ReadAsStringAsync();
            var teacher = JsonConvert.DeserializeObject<TeacherDashboardView>(teacherJson);

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!courseRes.IsSuccessStatusCode)
                return RedirectToAction("Dashboard", "Teacher");

            var cJson = await courseRes.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(cJson);

            if (course == null || course.TeacherID != teacher?.TeacherID)
            {
                TempData["Msg"] = "⚠️ You are not authorized to mark attendance for this course.";
                return RedirectToAction("Dashboard", "Teacher");
            }

            ViewBag.CourseID = course.CourseID;
            ViewBag.CourseName = course.CourseName;

            var studentRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (!studentRes.IsSuccessStatusCode)
            {
                TempData["Msg"] = "⚠️ No students found for this course.";
                return RedirectToAction(nameof(Index));
            }

            var json = await studentRes.Content.ReadAsStringAsync();
            var students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();

            var list = students.Select(s => new AttendanceView
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                CourseID = courseId,
                CourseName = course.CourseName,
                AttendanceDate = DateTime.Today,
                Status = "Present"
            }).ToList();

            return View("~/Views/Attendance/Create.cshtml", list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DateTime AttendanceDate, TimeSpan SessionTime, List<AttendanceView> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
                return RedirectToAction(nameof(Index));

            foreach (var record in attendanceList)
            {
                record.AttendanceDate = AttendanceDate + SessionTime;
                record.IsValidated = true;
                record.IsPresent = record.Status == "Present";

                var payload = JsonConvert.SerializeObject(record);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                await _client.PostAsync($"{_apiBase}Attendance", content);
            }

            TempData["Msg"] = "✅ Attendance saved successfully!";
            return RedirectToAction(nameof(Index), new { courseId = attendanceList.First().CourseID });
        }

        public async Task<IActionResult> Delete(int id, int? courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}Attendance/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<AttendanceView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index), new { courseId });

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}Attendance/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Attendance record deleted successfully."
                : "❌ Failed to delete attendance record.";

            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
