using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly HttpClient _client;

        public UsersController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("API");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? role = "All", string status = "All", string search = "")
        {
            var res = await _client.GetAsync("Users");
            if (!res.IsSuccessStatusCode)
                return View("Index", new List<UserView>());

            var json = await res.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserView>>(json) ?? new();

            var filtered = users.Where(u =>
                (role == "All" ||
                 (role == "Teacher" && u.RoleID == 2) ||
                 (role == "Student" && u.RoleID == 3) ||
                 (role == "Admin" && u.RoleID == 1))
                &&
                (status == "All" ||
                 (status == "Active" && u.IsActive) ||
                 (status == "Inactive" && !u.IsActive))
            ).ToList();

            foreach (var u in users)
            {
                if (role?.ToLower() == "teacher" && u.RoleID != 2) continue;
                if (role?.ToLower() == "student" && u.RoleID != 3) continue;
                if (role?.ToLower() == "admin" && u.RoleID != 1) continue;

                if (status == "Active" && !u.IsActive) continue;
                if (status == "Inactive" && u.IsActive) continue;

                filtered.Add(u);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.ToLower();
                filtered = filtered.Where(u =>
                            u.UserID.ToString().Contains(term) ||
                            (u.FullName?.ToLower().Contains(term) ?? false) ||
                            (u.Email?.ToLower().Contains(term) ?? false)
                         ).ToList();
            }

            ViewBag.RoleFilter = role;
            ViewBag.StatusFilter = status;
            ViewBag.SearchTerm = search;

            return View("Index", filtered);
        }
        // ===========================
        // ENROLL  (REDIRECT TO StudentCoursesController)
        // ===========================
        [HttpGet("Enroll/{id}")]
        public IActionResult Enroll(int id, string? role = "All", string status = "All")
        {
            return RedirectToAction(
                "Enroll",
                "StudentCourses",
                new { studentId = id, role, status }
            );
        }

        [HttpGet]
        public IActionResult Create(string? roleFilter = null)
        {
            ViewBag.RoleFilter = roleFilter ?? "All";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserView model, string roleFilter)
        {
            ViewBag.RoleFilter = roleFilter;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var apiModel = new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = model.Password,
                    RoleID = model.RoleID,
                    IsActive = model.IsActive
                };

            var json = JsonConvert.SerializeObject(apiModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await _client.PostAsync($"{_apiBase}Users/create", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = $"{roleFilter} created successfully!";
                return RedirectToAction("Index", new { role = roleFilter, status = "All" });
            }

                ViewBag.Error = "Failed to create user.";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? role = "All", string status = "All")
        {
            var res = await _client.GetAsync($"Users/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to fetch user.";
                return RedirectToAction("Index", new { role, status });
            }

            var json = await res.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserView>(json);

            ViewBag.RoleFilter = role;
            ViewBag.StatusFilter = status;

            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserView model, string roleFilter, string statusFilter)
        {
            ViewBag.RoleFilter = roleFilter;

            try
            {
                var payload = new
                {
                    model.UserID,
                    model.FullName,
                    model.Email,
                    PasswordHash = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password,
                    model.RoleID,
                    model.IsActive
                };

            var json = JsonConvert.SerializeObject(apiModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PutAsync("Users/update", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "User updated successfully!";
                return RedirectToAction("Index", new { role = roleFilter, status = statusFilter });
            }

                ViewBag.Error = "Failed to update user.";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, string? role = "All", string status = "All")
        {
            var res = await _client.GetAsync($"{_apiBase}Users/{id}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to fetch user.";
                return RedirectToAction("Index", new { role, status });
            }

            var json = await res.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserView>(json);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", new { role, status });
            }

            ViewBag.Role = role;
            ViewBag.Status = status;

            return View("Delete", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int userID, string role, string status = "All")
        {
            var res = await _client.DeleteAsync($"{_apiBase}Users/{userID}");

            if (res.IsSuccessStatusCode)
                TempData["Msg"] = "User deleted successfully!";
            else
                TempData["Msg"] = "Failed to delete user.";

            return RedirectToAction("Index", new { role, status });
        }

        [HttpGet]
        public IActionResult Enroll(int id, string? role = "All", string status = "All")
        {
            var res = await _client.DeleteAsync($"Users/{userID}");

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "User deleted successfully!"
                : "Failed to delete user.";

            return RedirectToAction("Index", new { role });
        }
    }
}
