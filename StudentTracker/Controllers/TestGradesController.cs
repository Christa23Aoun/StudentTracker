using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class TestGradesController : Controller
    {
        private readonly HttpClient _http;

        public TestGradesController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7199/api/"); // <- put YOUR API port
        }

        public async Task<IActionResult> Index()
        {
            var data = await _http.GetFromJsonAsync<List<TestGrade>>("TestGrades") ?? new();
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(TestGrade model)
        {
            await _http.PostAsJsonAsync("TestGrades", model);
            return RedirectToAction(nameof(Index));
        }

        // no Delete view – delete directly, then redirect
        public async Task<IActionResult> Delete(int id)
        {
            await _http.DeleteAsync($"TestGrades/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
