using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;

namespace StudentTracker.Controllers
{
    public class SiteController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public SiteController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public IActionResult About() => View();
        public IActionResult Admissions() => View();
        public IActionResult Research() => View();
        public IActionResult StudentLife() => View();
        public IActionResult News() => View();

        // ✅ Faculties Page — Show All Departments
        public async Task<IActionResult> Faculties()
        {
            try
            {
                var response = await _client.GetAsync($"{_apiBase}Departments");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to load departments from the server.";
                    return View(new List<DepartmentView>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<DepartmentView>>(json) ?? new();

                return View(departments);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
                return View(new List<DepartmentView>());
            }
        }
    }
}
