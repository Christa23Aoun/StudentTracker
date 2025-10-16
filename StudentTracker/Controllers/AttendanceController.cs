using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly HttpClient _http;

        public AttendanceController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7199/api/");
        }

        public async Task<IActionResult> Index()
        {
            var data = await _http.GetFromJsonAsync<List<Attendance>>("Attendance") ?? new();
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Attendance model)
        {
            await _http.PostAsJsonAsync("Attendance", model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _http.DeleteAsync($"Attendance/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
