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

        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Courses");

            if (!res.IsSuccessStatusCode)
                return View(new List<CourseView>());

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();

            return View(list);
        }

        private async Task PopulateLookupsAsync(CourseView model)
        {
            var depRes = await _client.GetAsync($"{_apiBase}Lookups/departments");
            if (depRes.IsSuccessStatusCode)
            {
                var json = await depRes.Content.ReadAsStringAsync();
                model.Departments = JsonConvert.DeserializeObject<List<LookupItem>>(json) ?? new();
            }

            var semRes = await _client.GetAsync($"{_apiBase}Lookups/semesters");
            if (semRes.IsSuccessStatusCode)
            {
                var json = await semRes.Content.ReadAsStringAsync();
                model.Semesters = JsonConvert.DeserializeObject<List<LookupItem>>(json) ?? new();
            }

            var teacherRes = await _client.GetAsync($"{_apiBase}Lookups/teachers");
            if (teacherRes.IsSuccessStatusCode)
            {
                var json = await teacherRes.Content.ReadAsStringAsync();
                model.Teachers = JsonConvert.DeserializeObject<List<LookupItem>>(json) ?? new();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CourseView
            {
                IsActive = true
            };

            await PopulateLookupsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseView model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Courses", content);

            if (!res.IsSuccessStatusCode)
            {
                TempData["CourseError"] = "Failed to create course.";
                await PopulateLookupsAsync(model);
                return View(model);
            }

            TempData["CourseSuccess"] = "Course created successfully.";
            return RedirectToAction("Index", "Courses");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CourseView>(json);

            if (model == null)
                return NotFound();

            await PopulateLookupsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseView model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PutAsync($"{_apiBase}Courses/{model.CourseID}", content);

            if (!res.IsSuccessStatusCode)
            {
                TempData["CourseError"] = "Failed to update course.";
                await PopulateLookupsAsync(model);
                return View(model);
            }

            TempData["CourseSuccess"] = "Course updated successfully.";
            return RedirectToAction("Index", "Courses");
        }

        [HttpGet]
        public async Task<IActionResult> Schedule(int courseId)
        {
            var list = new List<CourseScheduleView>();

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var jsonCourse = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(jsonCourse);
                ViewBag.CourseName = course?.CourseName;
            }

            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/course/{courseId}");
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<CourseScheduleView>>(json) ?? new();
            }

            ViewBag.CourseID = courseId;
            return View("Schedule", list);
        }

        [HttpGet]
        public IActionResult AddSchedule(int courseId)
        {
            var model = new CourseScheduleView
            {
                CourseID = courseId
            };

            return View("AddSchedule", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(CourseScheduleView model)
        {
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}CourseSchedule", content);

            if (!res.IsSuccessStatusCode)
                TempData["CourseError"] = "Failed to add schedule.";
            else
                TempData["CourseSuccess"] = "Schedule added successfully.";

            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> EditSchedule(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CourseScheduleView>(json);

            if (model == null)
                return NotFound();

            return View("EditSchedule", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(CourseScheduleView model)
        {
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PutAsync($"{_apiBase}CourseSchedule/{model.ScheduleID}", content);

            if (!res.IsSuccessStatusCode)
                TempData["CourseError"] = "Failed to update schedule.";
            else
                TempData["CourseSuccess"] = "Schedule updated successfully.";

            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteScheduleConfirmation(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CourseScheduleView>(json);

            if (model == null)
                return NotFound();

            return View("DeleteScheduleConfirmation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id, int courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}CourseSchedule/{id}");

            TempData["CourseSuccess"] = res.IsSuccessStatusCode
                ? "Schedule deleted successfully."
                : "Failed to delete schedule.";

            return RedirectToAction("Schedule", new { courseId });
        }

        [HttpGet]
        public async Task<IActionResult> DeactivateCourseConfirmation(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CourseView>(json);

            if (model == null)
                return NotFound();

            return View("DeactivateCourseConfirmation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateCourse(int id)
        {
            var res = await _client.PutAsync($"{_apiBase}Courses/deactivate/{id}", null);

            TempData["CourseSuccess"] = res.IsSuccessStatusCode
                ? "Course deactivated successfully."
                : "Failed to deactivate course.";

            return RedirectToAction(nameof(Index));
        }
    }
}
