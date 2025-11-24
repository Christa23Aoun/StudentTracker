using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    public class CoursesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public CoursesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // ============================
        // COURSES LIST
        // ============================
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Courses");

            if (!res.IsSuccessStatusCode)
                return View(new List<CourseView>());

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

            return View(list);
        }

        // ============================
        // COURSE SCHEDULE (LIST)
        // ============================
        public async Task<IActionResult> Schedule(int courseId)
        {
            if (courseId == 0)
                return BadRequest("Missing course ID");

            ViewBag.CourseID = courseId;

            // Fetch course name & teacher name
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var courseJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
                ViewBag.CourseName = course?.CourseName;
                ViewBag.TeacherName = course?.TeacherName;
            }

            // Fetch schedule entries
            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/course/{courseId}");
            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CourseScheduleView>>(json) ?? new();

            return View("~/Views/Courses/Schedule.cshtml", list);
        }

        // ============================
        // ADD SCHEDULE (GET)
        // ============================
        public IActionResult AddSchedule(int courseId)
        {
            if (courseId == 0)
                return BadRequest("Missing course ID");

            var vm = new CourseScheduleView
            {
                CourseID = courseId
            };

            return View(vm);
        }

        // ============================
        // ADD SCHEDULE (POST)
        // ============================
        [HttpPost]
        public async Task<IActionResult> AddSchedule(CourseScheduleView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // End time = start time + 1h15
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync($"{_apiBase}CourseSchedule", content);

            if (!res.IsSuccessStatusCode)
            {
                // The API returns 400 when there is a teacher conflict
                TempData["ScheduleError"] = "Teacher conflict detected. The selected time overlaps with another course.";
                return RedirectToAction("AddSchedule", new { courseId = model.CourseID });
            }

            TempData["ScheduleSuccess"] = "Schedule added successfully.";
            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

        // ============================
        // EDIT SCHEDULE (GET)
        // ============================
        public async Task<IActionResult> EditSchedule(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<CourseScheduleView>(json);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // ============================
        // EDIT SCHEDULE (POST)
        // ============================
        [HttpPost]
        public async Task<IActionResult> EditSchedule(int id, CourseScheduleView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // End time = start time + 1h15
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PutAsync($"{_apiBase}CourseSchedule/{id}", content);

            if (!res.IsSuccessStatusCode)
            {
                TempData["ScheduleError"] = "Teacher conflict detected. The selected time overlaps with another course.";
                return RedirectToAction("EditSchedule", new { id });
            }

            TempData["ScheduleSuccess"] = "Schedule updated successfully.";
            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

        // ============================
        // DELETE SCHEDULE
        // ============================
        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id, int courseId)
        {
            await _client.DeleteAsync($"{_apiBase}CourseSchedule/{id}");
            TempData["ScheduleSuccess"] = "Schedule deleted successfully.";
            return RedirectToAction("Schedule", new { courseId });
        }
    }
}
