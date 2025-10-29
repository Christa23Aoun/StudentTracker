using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("API");
        }

        // ✅ List users (optionally filtered by role)
        public async Task<IActionResult> Index(string? role)
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>("api/Users");
            if (users == null) return View(new List<User>());

            // Show only active users
            users = users.Where(u => u.IsActive).ToList();

            // ✅ Filter by role if provided
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u =>
                    (role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.RoleID == 3) ||
                    (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) && u.RoleID == 2)
                ).ToList();

                ViewBag.RoleFilter = role;
            }

            return View(users);
        }

        // ✅ Create user (GET)
        [HttpGet]
        public IActionResult Create() => View();

        // ✅ Create user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
                return View(user);
            }

            // ✅ Send password in plain text (same behavior as Swagger)
            var response = await _httpClient.PostAsJsonAsync("api/Users/create", user);

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ User created successfully!";

                // ✅ Redirect based on RoleID
                if (user.RoleID == 3)
                    return RedirectToAction("Index", new { role = "Student" });
                if (user.RoleID == 2)
                    return RedirectToAction("Index", new { role = "Teacher" });

                return RedirectToAction("Index");
            }

            // ❌ If API fails, show message
            ViewBag.Error = "❌ Unable to create user. Please check all fields.";
            return View(user);
        }

        // ✅ Edit user (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"api/Users/{id}");
            if (user == null)
                return NotFound();

            return View(user);
        }

        // ✅ Edit user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Users/update", user);
            if (response.IsSuccessStatusCode)
            {
                if (user.RoleID == 3)
                    return RedirectToAction("Index", new { role = "Student" });
                else if (user.RoleID == 2)
                    return RedirectToAction("Index", new { role = "Teacher" });
                else
                    return RedirectToAction("Index");
            }

            ViewBag.Error = "❌ Update failed.";
            return View(user);
        }

        // ✅ Delete user (GET confirmation)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"api/Users/{id}");
            if (user == null) return NotFound();

            ViewBag.Role = user.RoleID == 3 ? "Student" :
                           user.RoleID == 2 ? "Teacher" : "Admin";

            return View(user);
        }

        // ✅ Delete user (POST confirmed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int userID, string role)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{userID}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ User deleted successfully!";
                return RedirectToAction("Index", new { role });
            }

            TempData["Msg"] = "❌ Delete failed.";
            return RedirectToAction("Index", new { role });
        }
    }
}
