using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            /* ===================== STUDENT ===================== */
            if (isStudent)
            {
                var msg = (notification.Message ?? "").ToLower();

                if (msg.Contains("session"))
                    return Redirect("/StudentDashboard/Schedule");

                if (msg.Contains("enrolled"))
                    return Redirect("/StudentDashboard/Index");

                if (msg.Contains("profile") || msg.Contains("account"))
                    return Redirect("/StudentDashboard/Profile");

                if (!string.IsNullOrWhiteSpace(notification.TargetUrl))
                    return Redirect(notification.TargetUrl);

                return Redirect("/Notifications");
            }

            /* ===================== TEACHER ===================== */
            if (isTeacher)
            {
                var msg = (notification.Message ?? "").ToLower();

                /* CASE 1: TargetUrl EXISTS -> check assignment if course-related, then redirect */
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

                /* CASE 2: Course details updated but TargetUrl is Dashboard (or empty) -> resolve course then go details */
                if (msg.Contains("course") && msg.Contains("details") && msg.Contains("updated"))
                {
                    var courseId = await ResolveCourseIdFromMessage(userId.Value, notification.Message);
                    if (courseId.HasValue)
                    {
                        var assigned = await IsTeacherAssignedToCourse(userId.Value, courseId.Value);
                        if (!assigned)
                        {
                            TempData["Error"] =
                                "You are no longer assigned to this course and cannot view its content.";
                            return Redirect("/Teacher/Dashboard");
                        }

                        return Redirect($"/Teacher/CourseDetails/{courseId.Value}");
                    }
                }

                /* CASE 3: No TargetUrl (assigned/unassigned/profile updates etc.) -> show the message page */
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
                if (targetUrl.Contains("/"))
                {
                    var parts = targetUrl.Trim('/').Split('/');
                    var last = parts.Last();
                    if (int.TryParse(last, out var routeId))
                        return routeId;
                }

                var uri = new Uri("http://dummy" + targetUrl);
                var query = uri.Query.TrimStart('?').Split('&');

                foreach (var q in query)
                {
                    var kv = q.Split('=');
                    if (kv.Length == 2 &&
                        (kv[0].ToLower().Contains("courseid") || kv[0].ToLower() == "id"))
                        return int.TryParse(kv[1], out var id) ? id : null;
                }
            }
            catch { }

            return null;
        }

        private async Task<int?> ResolveCourseIdFromMessage(int teacherUserId, string message)
        {
            try
            {
                var start = message.IndexOf("\"");
                var end = message.LastIndexOf("\"");
                if (start == -1 || end <= start)
                    return null;

                var courseName = message.Substring(start + 1, end - start - 1).Trim();

                var res = await _client.GetAsync($"{_apiBase}TeacherDashboard/{teacherUserId}");
                if (!res.IsSuccessStatusCode)
                    return null;

                var json = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                var courses = obj["courses"] ?? obj["Courses"];
                if (courses == null)
                    return null;

                foreach (var c in courses)
                {
                    var name = c["courseName"] ?? c["CourseName"];
                    var id = c["courseID"] ?? c["CourseID"];
                    if (name != null && id != null &&
                        name.ToString().Equals(courseName, StringComparison.OrdinalIgnoreCase))
                        return id.Value<int>();
                }
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
