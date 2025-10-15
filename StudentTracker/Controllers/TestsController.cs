using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class TestsController : Controller
    {
        private readonly HttpClient _http;

        public TestsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7199/api/"); // 🔧 update port to your API
        }

        public async Task<IActionResult> Index()
        {
            var data = await _http.GetFromJsonAsync<List<Test>>("Tests");
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Test model)
        {
            await _http.PostAsJsonAsync("Tests", model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _http.DeleteAsync($"Tests/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
