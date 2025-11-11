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

        // ✅ Show attendance records (optionally filtered by course)
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var list = new List<AttendanceView>();

            string endpoint = courseId.HasValue
                ? $"{_apiBase}Attendance/byCourse/{courseId}"
                : $"{_apiBase}Attendance";

            var res = await _client.GetAsync(endpoint);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();
            }

            return View(list);
        }

        // ✅ Display attendance creation form (with optional course prefilled)
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var model = new AttendanceView();

            if (courseId.HasValue)
                model.CourseID = courseId.Value;

            return View(model);
        }

        // ✅ Handle attendance creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttendanceView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Attendance", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to add attendance record.");
                return View(model);
            }

            TempData["Msg"] = "✅ Attendance record created successfully.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }

        // ✅ Confirm deletion
        public async Task<IActionResult> Delete(int id, int? courseId)
        {
            ViewBag.CourseID = courseId;
            var res = await _client.GetAsync($"{_apiBase}Attendance/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<AttendanceView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index), new { courseId });

            return View(item);
        }

        // ✅ Delete attendance and redirect back
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

        // ✅ NEW: Step 1 — Display students of a course for marking
        [HttpGet]
        public async Task<IActionResult> Mark(int courseId)
        {
            ViewBag.CourseID = courseId;

            var studentRes = await _client.GetAsync($"{_apiBase}StudentCourse/byCourse/{courseId}");
            if (!studentRes.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await studentRes.Content.ReadAsStringAsync();
            var students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();

            var list = students.Select(s => new AttendanceView
            {
                StudentID = s.StudentID,
                CourseID = courseId,
                StudentName = s.StudentName,
                AttendanceDate = DateTime.Today
            }).ToList();

            return View("~/Views/Attendance/Mark.cshtml", list);
        }

        // ✅ NEW: Step 2 — Save attendance for all students
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(DateTime AttendanceDate, TimeSpan SessionTime, List<AttendanceView> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
                return RedirectToAction(nameof(Index));

            foreach (var record in attendanceList)
            {
                record.AttendanceDate = AttendanceDate + SessionTime;
                var payload = JsonConvert.SerializeObject(record);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                await _client.PostAsync($"{_apiBase}Attendance", content);
            }

            TempData["Msg"] = "✅ Attendance saved successfully!";
            return RedirectToAction(nameof(Index), new { courseId = attendanceList.First().CourseID });
        }
    }
}
