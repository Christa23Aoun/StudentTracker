using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
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
            _client = factory.CreateClient("API"); // ✅ use named client ("API")
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // ---------- LOGIN (GET) ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? role = null)
        {
            ViewBag.Role = role ?? "Student";
            return View();
        }

        // ---------- LOGIN (POST) ----------
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            Console.WriteLine("➡️ Login POST triggered");

            // ✅ Debug ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Invalid model state");
                foreach (var kv in ModelState)
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        Console.WriteLine($"❌ Field: {kv.Key} - Error: {err.ErrorMessage}");
                    }
                }

                // Keep the selected role visible
                ViewBag.Role = model.Role ?? "Student";
                return View(model);
            }

            try
            {
                // 🔹 Prepare payload for API
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{_apiBase}Auth/login", content);

                Console.WriteLine($"🔍 Login API response: {res.StatusCode}");
                string apiReply = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"🔍 API reply body: {apiReply}");

                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid email or password.";
                    ViewBag.Role = model.Role ?? "Student";
                    return View(model);
                }

                // 🔹 Fetch user details by email
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

                // 🔹 Determine RoleName based on RoleID
                string roleName = user.RoleID switch
                {
                    1 => "Admin",
                    2 => "Teacher",
                    3 => "Student",
                    _ => "Unknown"
                };

                // 🔹 Save session
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", roleName);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);

                // 🔹 Authenticate cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName)
                };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("CookieAuth", principal);

                // ✅ Redirect based on role
                return roleName switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                    "Student" => RedirectToAction("Index", "StudentDashboard"),
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
        public IActionResult LoginStudent()
        {
            ViewBag.Role = "Student";
            return View("Login");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginTeacher()
        {
            ViewBag.Role = "Teacher";
            return View("Login");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginAdmin()
        {
            ViewBag.Role = "Admin";
            return View("Login");
        }

        // ---------- REGISTER ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterView model)
        {
            Console.WriteLine($"📧 Email: {model.Email}");
            Console.WriteLine($"🔑 Password: {model.Password}");
            Console.WriteLine($"🎭 Role: {model.Role}");

            if (!ModelState.IsValid)
            {
                foreach (var kv in ModelState)
                {
                    foreach (var err in kv.Value.Errors)
                    {
                        Console.WriteLine($"❌ Field: {kv.Key} - Error: {err.ErrorMessage}");
                    }
                }
                return View(model);
            }

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
        public async Task<IActionResult> Logout()
        {
            // 🧹 Clear session
            HttpContext.Session.Clear();

            // 🧹 Sign out from cookie authentication
            await HttpContext.SignOutAsync("CookieAuth");

            // ✅ Redirect to login page
            return RedirectToAction("Login", "Auth");
        }

    }
}
