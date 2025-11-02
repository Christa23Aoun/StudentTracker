using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only logged-in users can access student course management
    [Authorize]
    public class StudentCoursesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentCoursesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /StudentCourses
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses");
            if (!res.IsSuccessStatusCode)
                return View(new List<StudentCourseView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            return View(data);
        }

        // GET: /StudentCourses/Create
        public IActionResult Create() => View();

        // POST: /StudentCourses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCourseView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}StudentCourses", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to add student course record.");
                return View(model);
            }

            TempData["Msg"] = "Student course created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /StudentCourses/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<StudentCourseView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        // POST: /StudentCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}StudentCourses/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Student course deleted successfully."
                : "Failed to delete student course.";

            return RedirectToAction(nameof(Index));
        }
    }
}
