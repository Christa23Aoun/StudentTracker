using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTrackerCOMMON.DTOs.AdminDashboard;

namespace StudentTracker.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AdminController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var client = _clientFactory.CreateClient("API");

            try
            {
                // ✅ FIXED: correct API route (matches your DashboardController)
                var response = await client.GetAsync("api/dashboard/admin/summary");

                var json = await response.Content.ReadAsStringAsync();

                // 🧩 Print to Output window for debugging
                Console.WriteLine("=== API DASHBOARD RESPONSE ===");
                Console.WriteLine(json);
                Console.WriteLine("=== END OF RESPONSE ===");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Msg"] = $"⚠️ API call failed: {response.StatusCode}";
                    return View("~/Views/Dashboard/Admin.cshtml", new AdminFullDashboardDto());
                }

                var dashboardData = JsonConvert.DeserializeObject<AdminFullDashboardDto>(json);

                if (dashboardData == null)
                {
                    TempData["Msg"] = "❌ Deserialization failed (null object)";
                    dashboardData = new AdminFullDashboardDto();
                }

                return View("~/Views/Dashboard/Admin.cshtml", dashboardData);
            }
            catch (Exception ex)
            {
                TempData["Msg"] = $"❌ Error: {ex.Message}";
                return View("~/Views/Dashboard/Admin.cshtml", new AdminFullDashboardDto());
            }
        }
    }
}
