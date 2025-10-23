using Microsoft.AspNetCore.Mvc;
using StudentTracker.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        // constructor: receives the HttpClient we registered in Program.cs
        public DashboardController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("API");
        }

        // this calls your API: GET /api/dashboard/admin/summary
        public async Task<IActionResult> Admin()
        {
            var response = await _httpClient.GetAsync("api/dashboard/admin/summary");

            if (!response.IsSuccessStatusCode)
                return View("Error"); // simple error fallback

            var json = await response.Content.ReadAsStringAsync();

            var summary = JsonSerializer.Deserialize<DashboardSummaryViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(summary);
        }
    }
}
