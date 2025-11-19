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
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (!courseId.HasValue)
                return View(new List<AttendanceView>());

            var res = await _client.GetAsync($"{_apiBase}Attendance/byCourse/{courseId}");
            var list = new List<AttendanceView>();

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();
            }

            ViewBag.CourseID = courseId;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!courseRes.IsSuccessStatusCode)
                return RedirectToAction("Dashboard", "Teacher");

            var courseJson = await courseRes.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(courseJson);

            ViewBag.CourseID = courseId;
            ViewBag.CourseName = course?.CourseName ?? "";

            var studentRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            var students = new List<StudentCourseView>();

            if (studentRes.IsSuccessStatusCode)
            {
                var sJson = await studentRes.Content.ReadAsStringAsync();
                students = JsonConvert.DeserializeObject<List<StudentCourseView>>(sJson) ?? new();
            }

            var list = students.Select(s => new AttendanceView
            {
                StudentID = s.StudentID,
                StudentName = s.StudentName,
                CourseID = courseId,
                CourseName = course?.CourseName ?? "",
                AttendanceDate = DateTime.Today,
                Status = "Present"
            }).ToList();

            return View(list);
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

                // Convert Status → IsPresent + IsValidated
                record.IsPresent = record.Status != "Absent";
                record.IsValidated = true;

                var payload = JsonConvert.SerializeObject(record);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                await _client.PostAsync($"{_apiBase}Attendance", content);
            }

            TempData["Msg"] = "✅ Attendance saved successfully!";
            return RedirectToAction(nameof(Index), new { courseId = attendanceList.First().CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int? courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}Attendance/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<AttendanceView>(json);

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? courseId)
        {
            await _client.DeleteAsync($"{_apiBase}Attendance/{id}");
            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
