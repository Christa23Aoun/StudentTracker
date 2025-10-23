using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class TeacherController : Controller
    {
        private readonly HttpClient _http;

        public TeacherController(IHttpClientFactory factory, IConfiguration config)
        {
            // Create named HttpClient
            _http = factory.CreateClient("API");
        }

        // ✅ GET: Teacher Dashboard
        public async Task<IActionResult> Dashboard(int teacherId = 2) // temporary ID until login integration
        {
            try
            {
                // 1️⃣ Call the unified API endpoint
                var dashboard = await _http.GetFromJsonAsync<TeacherDashboardDto>(
                    $"api/TeacherDashboard/Teacher/{teacherId}");

                // 2️⃣ Send data to the View
                return View(dashboard);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"⚠️ Could not load dashboard: {ex.Message}";
                return View(new TeacherDashboardDto());
            }
        }
    }
}
