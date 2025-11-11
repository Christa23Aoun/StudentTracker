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

        // ✅ Show all tests (optionally filtered by course)
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

        // ✅ Display test creation form (with optional course prefilled)
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var model = new TestView();

            if (courseId.HasValue)
                model.CourseID = courseId.Value;

            return View(model);
        }

        // ✅ Handle test creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{_apiBase}Tests", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ Test created successfully!";
                return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
            }

            ViewBag.Error = "Failed to create test.";
            return View(model);
        }

        // ✅ Confirm deletion
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

        // ✅ Delete test permanently
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}Tests/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Test deleted successfully."
                : "Failed to delete test.";

            return RedirectToAction(nameof(Index));
        }
    }
}
