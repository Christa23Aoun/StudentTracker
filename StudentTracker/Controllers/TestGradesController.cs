using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TestGradesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TestGradesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // ✅ Show list of grades (optionally by course)
        public async Task<IActionResult> Index(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var list = new List<TestGradeView>();

            string endpoint = courseId.HasValue
                ? $"{_apiBase}TestGrades/byCourse/{courseId}"
                : $"{_apiBase}TestGrades";

            var res = await _client.GetAsync(endpoint);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            return View(list);
        }

        // ✅ Display grade creation form (with optional course prefilled)
        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var model = new TestGradeView();

            if (courseId.HasValue)
                model.CourseID = courseId.Value;

            return View(model);
        }

        // ✅ Handle grade creation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestGradeView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_apiBase}TestGrades", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Msg"] = "✅ Test grade added successfully!";
                    return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
                }

                ViewBag.Error = "Failed to save test grade.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(model);
        }

        // ✅ Confirm deletion
        public async Task<IActionResult> Delete(int id, int? courseId)
        {
            ViewBag.CourseID = courseId;

            var res = await _client.GetAsync($"{_apiBase}TestGrades/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<TestGradeView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index), new { courseId });

            return View(item);
        }

        // ✅ Delete grade and redirect back
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Test grade deleted successfully."
                : "Failed to delete test grade.";

            return RedirectToAction(nameof(Index), new { courseId });
        }
    }
}
