using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only authorized users (typically Admins) can manage accounts
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public UsersController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // ✅ List users (optionally filtered by role)
        public async Task<IActionResult> Index(string? role)
        {
            var res = await _client.GetAsync($"{_apiBase}Users");
            if (!res.IsSuccessStatusCode)
                return View(new List<UserView>());

            var json = await res.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserView>>(json) ?? new();

            // show only active
            users = users.Where(u => u.IsActive).ToList();

            // filter by role if provided
            if (!string.IsNullOrWhiteSpace(role))
            {
                users = users
                    .Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
                    .ToList();
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
        public async Task<IActionResult> Create(UserView model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
                return View(model);
            }

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Users/create", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ User created successfully!";
                return RedirectToAction(nameof(Index), new { role = model.Role });
            }

            ViewBag.Error = "❌ Unable to create user. Please check the data.";
            return View(model);
        }

        // ✅ Edit user (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Users/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserView>(json);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // ✅ Edit user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserView model)
        {
            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PutAsync($"{_apiBase}Users/update", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ User updated successfully!";
                return RedirectToAction(nameof(Index), new { role = model.Role });
            }

            ViewBag.Error = "❌ Update failed.";
            return View(model);
        }

        // ✅ Delete user (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Users/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserView>(json);
            if (user == null)
                return NotFound();

            ViewBag.Role = user.Role;
            return View(user);
        }

        // ✅ Delete user (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int userId, string role)
        {
            var res = await _client.DeleteAsync($"{_apiBase}Users/{userId}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ User deleted successfully!"
                : "❌ Delete failed.";

            return RedirectToAction(nameof(Index), new { role });
        }
    }
}
