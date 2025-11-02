using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AuthController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // ---------- LOGIN ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // 🔹 Send login request to API
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{_apiBase}Auth/login", content);

                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View(model);
                }

                // 🔹 After successful login, retrieve user details by email
                var userRes = await _client.GetAsync($"{_apiBase}Users/email/{model.Email}");
                if (!userRes.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to fetch user details.";
                    return View(model);
                }

                var json = await userRes.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserView>(json);
                if (user == null)
                {
                    ViewBag.Error = "User not found.";
                    return View(model);
                }

                // 🔹 Map RoleName based on RoleID (DB convention)
                string roleName = user.RoleID switch
                {
                    1 => "Admin",
                    2 => "Teacher",
                    3 => "Student",
                    _ => "Unknown"
                };

                // 🔹 Save session info
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", roleName);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);
                Console.WriteLine($"✅ Logged in user: {user.FullName} ({user.Email}) | RoleID={user.RoleID}");
                Console.WriteLine($"Session set: " +
                    $"Name={HttpContext.Session.GetString("UserName")}, " +
                    $"Role={HttpContext.Session.GetString("UserRole")}, " +
                    $"ID={HttpContext.Session.GetInt32("UserID")}");


                // 🔹 Redirect based on role
                return user.RoleID switch
                {
                    1 => RedirectToAction("Dashboard", "Admin"),
                    2 => RedirectToAction("Dashboard", "Teacher"),
                    3 => RedirectToAction("Index", "StudentDashboard"),
                    _ => RedirectToAction("Login")
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server connection error: " + ex.Message;
                return View(model);
            }
        }

        // ---------- ROLE ENTRY POINTS ----------
        [HttpGet, AllowAnonymous]
        public IActionResult LoginStudent() { ViewBag.Role = "Student"; return View("Login"); }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginTeacher() { ViewBag.Role = "Teacher"; return View("Login"); }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginAdmin() { ViewBag.Role = "Admin"; return View("Login"); }

        // ---------- REGISTER ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{_apiBase}Auth/register", content);

                if (res.IsSuccessStatusCode)
                {
                    TempData["Msg"] = "Registration successful! Please log in.";
                    return RedirectToAction("Login");
                }

                ViewBag.Error = "Registration failed. Please try again.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(model);
        }

        // ---------- LOGOUT ----------
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
