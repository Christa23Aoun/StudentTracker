using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // Only logged-in (authorized) users can access the Admin Dashboard
   // [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AdminController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /admin/dashboard
        [HttpGet("/admin/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Call the API to fetch dashboard data
            var res = await _client.GetAsync($"{_apiBase}Dashboard/Admin");
            if (!res.IsSuccessStatusCode)
            {
                // If the API fails, show an empty dashboard
                return View("~/Views/Dashboard/Admin.cshtml", new AdminDashboardViewModel());
            }

            // Deserialize API response into your FrontEnd model
            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<AdminDashboardViewModel>(json)
                         ?? new AdminDashboardViewModel();

            return View("~/Views/Dashboard/Admin.cshtml", model);
        }
    }
}
