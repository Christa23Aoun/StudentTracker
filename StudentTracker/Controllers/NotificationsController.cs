using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public NotificationsController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!.TrimEnd('/') + "/";
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var res = await _client.GetAsync($"{_apiBase}Notifications/user/{userId}");
            if (!res.IsSuccessStatusCode)
                return View(new List<NotificationView>());

            var json = await res.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<List<NotificationView>>(json) ?? new();

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var roleId = HttpContext.Session.GetInt32("RoleID");
            var role = (HttpContext.Session.GetString("Role") ?? "").Trim().ToLower();
            var isStudent = (roleId.HasValue && roleId.Value == 3) || role.Contains("student");

            var res = await _client.GetAsync($"{_apiBase}Notifications/user/{userId}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<List<NotificationView>>(json) ?? new();

            var notification = notifications.FirstOrDefault(n => n.NotificationID == id);
            if (notification == null)
                return RedirectToAction(nameof(Index));

            await _client.PostAsync($"{_apiBase}Notifications/{id}/read", null);

            if (isStudent)
            {
                var msg = (notification.Message ?? "").ToLower();

                if (msg.Contains("test") || msg.Contains("grade") || msg.Contains("attendance"))
                {
                    if (!string.IsNullOrWhiteSpace(notification.TargetUrl))
                        return Redirect(notification.TargetUrl);
                }

                if (msg.Contains("session") || msg.Contains("class session"))
                    return Redirect("/StudentDashboard/Schedule");

                if (msg.Contains("enrolled"))
                    return Redirect("/StudentDashboard/Index");

                if (msg.Contains("profile") || msg.Contains("account"))
                    return Redirect("/StudentDashboard/Profile");

                if (!string.IsNullOrWhiteSpace(notification.TargetUrl))
                    return Redirect(notification.TargetUrl);

                return Redirect("/Notifications");
            }

            if (!string.IsNullOrWhiteSpace(notification.TargetUrl))
                return Redirect(notification.TargetUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
