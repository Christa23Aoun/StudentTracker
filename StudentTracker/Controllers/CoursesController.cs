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

        
        public async Task<IActionResult> Schedule(int courseId)
        {
            if (courseId == 0)
                return BadRequest("Missing course ID");

            ViewBag.CourseID = courseId;

           
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var courseJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
                ViewBag.CourseName = course?.CourseName;
                ViewBag.TeacherName = course?.TeacherName;
            }

            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/course/{courseId}");
            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CourseScheduleView>>(json) ?? new();

            return View("~/Views/Courses/Schedule.cshtml", list);
        }

        
       
        public IActionResult AddSchedule(int courseId)
        {
            return View(new CourseScheduleView { CourseID = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSchedule(CourseScheduleView model)
        {
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);

            var res = await _client.PostAsync(
                $"{_apiBase}CourseSchedule",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create schedule.");
                return View(model);
            }

            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

      
        public async Task<IActionResult> EditSchedule(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}CourseSchedule/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<CourseScheduleView>(json);

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> EditSchedule(int id, CourseScheduleView model)
        {
            // Auto calculate end time = +1h15
            model.EndTime = model.StartTime.Add(new TimeSpan(1, 15, 0));

            var payload = JsonConvert.SerializeObject(model);

            var res = await _client.PutAsync(
                $"{_apiBase}CourseSchedule/{id}",
                new StringContent(payload, Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Update failed.");
                return View(model);
            }

            return RedirectToAction("Schedule", new { courseId = model.CourseID });
        }

     
        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id, int courseId)
        {
            await _client.DeleteAsync($"{_apiBase}CourseSchedule/{id}");
            return RedirectToAction("Schedule", new { courseId });
        }
    }
}
