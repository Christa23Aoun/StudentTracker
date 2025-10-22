using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly HttpClient _httpClient;
        public NotificationsController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("API");
        }

        public async Task<IActionResult> Index(int userId = 1)
        {
            var notifications = await _httpClient.GetFromJsonAsync<List<Notification>>($"api/Notifications/user/{userId}");
            return View(notifications);
        }
    }
}
