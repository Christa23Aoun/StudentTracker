using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class StudentCoursesController : Controller
    {
        private readonly HttpClient _http;

        public StudentCoursesController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7199/api/"); // 👈 change to your API port
        }

        public async Task<IActionResult> Index()
        {
            var data = await _http.GetFromJsonAsync<List<StudentCourse>>("StudentCourses");
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(StudentCourse model)
        {
            await _http.PostAsJsonAsync("StudentCourses", model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _http.DeleteAsync($"StudentCourses/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
