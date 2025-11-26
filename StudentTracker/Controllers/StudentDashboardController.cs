using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using StudentTracker.Models.ViewModels;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class StudentDashboardController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentDashboardController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value
                       ?? "https://localhost:7199/api/";
        }

        // ==============================
        // MAIN DASHBOARD (already done)
        // ==============================
        public async Task<IActionResult> Index(int? studentId, string? semester, string? department)
        {
            var id = studentId ?? HttpContext.Session.GetInt32("StudentId");
            if (id == null)
                return RedirectToAction("Login", "Auth");

            var model = new StudentDashboardVM();

            try
            {
                var endpoint =
                    $"{_apiBase}Dashboard/Student/{id}?semester={semester}&department={department}";

                var response = await _client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return View(model);

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<StudentDashboardVM>(json)
                        ?? new StudentDashboardVM();
            }
            catch
            {
                return View(new StudentDashboardVM());
            }

            return View(model);
        }

        // =======================================
        // NEW: COURSE DETAILS FOR THE STUDENT
        // =======================================
        public async Task<IActionResult> CourseDetails(int courseId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");
            Console.WriteLine($"[DEBUG] StudentID={studentId}, CourseID={courseId}");

            var vm = new StudentCourseDetailsVM();

            try
            {
                var endpoint =
                    $"{_apiBase}Dashboard/Student/{studentId}/course/{courseId}";

                var res = await _client.GetAsync(endpoint);
                if (!res.IsSuccessStatusCode)
                    return View(vm);

                var json = await res.Content.ReadAsStringAsync();
                vm = JsonConvert.DeserializeObject<StudentCourseDetailsVM>(json)
                     ?? new StudentCourseDetailsVM();
            }
            catch
            {
                // on error return empty view instead of crashing
                return View(new StudentCourseDetailsVM());
            }

            return View(vm);
        }
    }
}
