using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only authorized (teachers/admins) can manage tests
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

        // GET: /Tests
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Tests");
            if (!res.IsSuccessStatusCode)
                return View(new List<TestView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            return View(data);
        }

        // GET: /Tests/Create
        public IActionResult Create() => View();

        // POST: /Tests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Tests", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create test.");
                return View(model);
            }

            TempData["Msg"] = "Test created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Tests/Delete/5
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

        // POST: /Tests/Delete/5
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
