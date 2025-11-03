using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only authorized users (teachers/admins) can access grades
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

        // GET: /TestGrades
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}TestGrades");
            if (!res.IsSuccessStatusCode)
                return View(new List<TestGradeView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            return View(data);
        }

        // GET: /TestGrades/Create
        public IActionResult Create() => View();

        // POST: /TestGrades/Create
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
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Failed to save test grade.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(model);
        }

        // GET: /TestGrades/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}TestGrades/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<TestGradeView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        // POST: /TestGrades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Test grade deleted successfully."
                : "Failed to delete test grade.";

            return RedirectToAction(nameof(Index));
        }
    }
}
