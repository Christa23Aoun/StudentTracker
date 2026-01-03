using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var role = (HttpContext.Session.GetString("Role") ?? "").ToLower();

            var isStudent = roleId == 3 || role.Contains("student");
            var isTeacher = roleId == 2 || role.Contains("teacher");

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
                var target = (notification.TargetUrl ?? "").Trim();
                var courseId = ExtractCourseId(target);

                if (msg.Contains("test") || msg.Contains("grade") || msg.Contains("attendance"))
                {
                    if (courseId.HasValue)
                        return Redirect($"/StudentDashboard/CourseDetails?courseId={courseId.Value}");

                    return Redirect("/StudentDashboard/Index");
                }

                if (msg.Contains("session") || msg.Contains("scheduled"))
                    return Redirect("/StudentDashboard/Schedule");

                if (msg.Contains("enrolled"))
                    return Redirect("/StudentDashboard/Index");

                if (msg.Contains("profile") || msg.Contains("account"))
                    return Redirect("/StudentDashboard/Profile");

                if (!string.IsNullOrWhiteSpace(target))
                {
                    if (target.StartsWith("/studentdashboard", StringComparison.OrdinalIgnoreCase))
                        return Redirect(target);

                    if (courseId.HasValue)
                        return Redirect($"/StudentDashboard/CourseDetails?courseId={courseId.Value}");
                }

                return RedirectToAction(nameof(Index));
            }

            if (isTeacher)
            {
                if (!string.IsNullOrWhiteSpace(notification.TargetUrl))
                {
                    var courseId = ExtractCourseId(notification.TargetUrl);
                    if (courseId.HasValue)
                    {
                        var assigned = await IsTeacherAssignedToCourse(userId.Value, courseId.Value);
                        if (!assigned)
                        {
                            TempData["Error"] =
                                "You are no longer assigned to this course and cannot view its content.";
                            return Redirect("/Teacher/Dashboard");
                        }
                    }

                    return Redirect(notification.TargetUrl);
                }

                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var res = await _client.GetAsync($"{_apiBase}Notifications/user/{userId}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<List<NotificationView>>(json) ?? new();

            var notification = notifications.FirstOrDefault(n => n.NotificationID == id);
            if (notification == null)
                return RedirectToAction(nameof(Index));

            return View(notification);
        }

        private int? ExtractCourseId(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
                return null;

            try
            {
                if (targetUrl.Contains("?"))
                {
                    var uri = new Uri("http://dummy" + targetUrl);
                    var query = uri.Query.TrimStart('?').Split('&');
                    foreach (var q in query)
                    {
                        var kv = q.Split('=');
                        if (kv.Length == 2 && kv[0].ToLower() == "courseid")
                            return int.TryParse(kv[1], out var id) ? id : null;
                    }
                }

                var parts = targetUrl.Trim('/').Split('/');
                var last = parts.Last();
                if (int.TryParse(last, out var routeId))
                    return routeId;
            }
            catch { }

            return null;
        }

        private async Task<bool> IsTeacherAssignedToCourse(int teacherUserId, int courseId)
        {
            try
            {
                var res = await _client.GetAsync($"{_apiBase}Courses/ByTeacher/{teacherUserId}");
                if (!res.IsSuccessStatusCode)
                    return false;

                var json = await res.Content.ReadAsStringAsync();
                var courses = JArray.Parse(json);

                return courses.Any(c =>
                {
                    var idToken = c["courseID"] ?? c["CourseID"];
                    return idToken != null && idToken.Value<int>() == courseId;
                });
            }
            catch
            {
                return false;
            }
        }
    }
}
