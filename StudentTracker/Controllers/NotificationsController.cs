using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only logged-in users can see notifications
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public NotificationsController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /Notifications
        public async Task<IActionResult> Index()
        {
            // Retrieve the logged-in user ID from session
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "You must be logged in to view notifications.";
                return RedirectToAction("Login", "Auth");
            }

            // Call API
            var res = await _client.GetAsync($"{_apiBase}Notifications/user/{userId}");
            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load notifications.";
                return View(new List<NotificationView>());
            }

            var json = await res.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<List<NotificationView>>(json) ?? new();
            return View(notifications);
        }
    }
}
