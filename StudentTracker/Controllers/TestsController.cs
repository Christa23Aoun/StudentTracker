using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TestsController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var list = new List<TestView>();

            string endpoint = courseId.HasValue
                ? $"{_apiBase}Tests/byCourse/{courseId}"
                : $"{_apiBase}Tests";

            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            }

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            ViewBag.CourseID = courseId;

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            string courseName = "";

            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                courseName = course?.CourseName ?? "";
            }

            ViewBag.CourseName = courseName;

            var model = new TestView
            {
                CourseID = courseId
            };

            return View(model);
        }

        private async Task LoadCourseName(int courseId)
        {
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            ViewBag.CourseID = courseId;
        }

        // ----------------------------------------------------------
        // CREATE TEST (POST) — WITH ERROR MESSAGE SUPPORT
        // ----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestView model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseName(model.CourseID);
                return View(model);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_apiBase}Tests", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ Test created successfully!";
                return RedirectToAction("Index", new { courseId = model.CourseID });
            }

            // ⭐⭐ THIS IS THE PART YOU DIDN’T UNDERSTAND ⭐⭐
            string error = await response.Content.ReadAsStringAsync();
            ViewBag.Error = error;

            await LoadCourseName(model.CourseID);
            return View(model);
        }

        // ----------------------------------------------------------
        // EDIT TEST (GET)
        // ----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Tests/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToAction("Index");

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TestView>(json);

            return View(model);
        }

        // ----------------------------------------------------------
        // EDIT TEST (POST) — WITH ERROR MESSAGE SUPPORT
        // ----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PutAsync($"{_apiBase}Tests", content);

            if (res.IsSuccessStatusCode)
                return RedirectToAction("Index", new { courseId = model.CourseID });

            // ⭐⭐ NEW: DISPLAY ERROR FROM API (weight > 100) ⭐⭐
            string error = await res.Content.ReadAsStringAsync();
            ViewBag.Error = error;

            return View(model);
        }

        // ----------------------------------------------------------
        // DELETE (GET)
        // ----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Tests/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<TestView>(json);

            if (test == null)
                return RedirectToAction(nameof(Index));

            return View(test);
        }

        // ----------------------------------------------------------
        // DELETE (POST)
        // ----------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Tests/Delete")]
        public async Task<IActionResult> DeleteConfirmed(int TestID, int CourseID)
        {
            await _client.DeleteAsync($"{_apiBase}Tests/{TestID}");
            return RedirectToAction(nameof(Index), new { courseId = CourseID });
        }
    }
}
