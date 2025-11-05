using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;

namespace StudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AdminController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        [HttpGet("/admin/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var res = await _client.GetAsync($"{_apiBase}Dashboard/Admin");

                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to fetch admin dashboard data.";
                    return View("~/Views/Dashboard/Admin.cshtml", new AdminDashboardViewModel());
                }

                var json = await res.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<AdminDashboardViewModel>(json)
                             ?? new AdminDashboardViewModel();

                return View("~/Views/Dashboard/Admin.cshtml", model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Server error: {ex.Message}";
                return View("~/Views/Dashboard/Admin.cshtml", new AdminDashboardViewModel());
            }
        }
    }
}
