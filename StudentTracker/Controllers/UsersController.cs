using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Collections.Generic;

namespace StudentTracker.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> Index(string? role = "All", string status = "All")
        {
            var res = await _client.GetAsync($"{_apiBase}Users");
            if (!res.IsSuccessStatusCode)
                return View("Index", new List<UserView>());

            var json = await res.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserView>>(json) ?? new();

            var filtered = new List<UserView>();
            foreach (var u in users)
            {
                if (role?.ToLower() == "teacher" && u.RoleID != 2) continue;
                if (role?.ToLower() == "student" && u.RoleID != 3) continue;
                if (role?.ToLower() == "admin" && u.RoleID != 1) continue;

                if (status == "Active" && !u.IsActive) continue;
                if (status == "Inactive" && u.IsActive) continue;

                filtered.Add(u);
            }

            ViewBag.RoleFilter = role;
            ViewBag.StatusFilter = status;

            return View("Index", filtered);
        }

        [HttpGet]
        public IActionResult Enroll(int id, string? role = "All", string status = "All")
        {
            return RedirectToAction("Enroll", "StudentCourses", new { studentId = id, role = role, status = status });
        }
    }
}
