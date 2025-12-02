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
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value
                       ?? "https://localhost:7199/api/";
        }

        // ============================================
        // LOGIN – STUDENT
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginStudent()
        {
            ViewBag.Role = "Student";
            ViewData["Title"] = "Login - Student";
            return View("Login");
        }

        // ============================================
        // LOGIN – TEACHER
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginTeacher()
        {
            ViewBag.Role = "Teacher";
            ViewData["Title"] = "Login - Teacher";
            return View("Login");
        }

        // ============================================
        // LOGIN – ADMIN
        // ============================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginAdmin()
        {
            ViewBag.Role = "Admin";
            ViewData["Title"] = "Login - Admin";
            return View("Login");
        }

        // ============================================
        // LOGIN POST (REMEMBER ME ENABLED)
        // ============================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            ViewBag.Role = model.Role;

            if (!ModelState.IsValid)
                return View("Login", model);

            try
            {
                // API LOGIN
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{_apiBase}Auth/login", content);

                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View("Login", model);
                }

                // FETCH USER DATA
                var userRes = await _client.GetAsync($"{_apiBase}Users/email/{model.Email}");
                if (!userRes.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to fetch user details.";
                    return View("Login", model);
                }

                var json = await userRes.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserView>(json);

                if (user == null)
                {
                    ViewBag.Error = "User not found.";
                    return View("Login", model);
                }

                // Resolve role
                string roleName = user.RoleID switch
                {
                    1 => "Admin",
                    2 => "Teacher",
                    3 => "Student",
                    _ => "Unknown"
                };

                // Block wrong role login
                if (!string.IsNullOrWhiteSpace(model.Role) &&
                    !string.Equals(roleName, model.Role, StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = $"This account belongs to a {roleName}. You cannot log in as a {model.Role}.";
                    return View("Login", model);
                }

                // Save session
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", roleName);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);

                if (roleName == "Student")
                    HttpContext.Session.SetInt32("StudentId", user.UserID);

                if (roleName == "Teacher")
                {
                    var tRes = await _client.GetAsync($"{_apiBase}TeacherDashboard/byEmail/{user.Email}");
                    if (tRes.IsSuccessStatusCode)
                    {
                        var tJson = await tRes.Content.ReadAsStringAsync();
                        var teacher = JsonConvert.DeserializeObject<TeacherView>(tJson);
                        if (teacher != null)
                            HttpContext.Session.SetInt32("TeacherID", teacher.TeacherID);
                    }
                }

                // AUTH cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,                          // browser will save credentials
                    ExpiresUtc = DateTime.UtcNow.AddDays(14)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                // Redirect
                return roleName switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Admin"),
                    "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                    "Student" => RedirectToAction("Index", "StudentDashboard"),
                    _ => RedirectToAction("LoginStudent")
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
                return View("Login", model);
            }
        }

        // ============================================
        // REGISTER
        // ============================================
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

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Auth/register", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Registration successful!";
                return RedirectToAction("LoginStudent");
            }

            ViewBag.Error = "Registration failed.";
            return View(model);
        }

        // ============================================
        // LOGOUT
        // ============================================
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            // FIXED: Use the default cookie scheme ("Cookies")
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

    }
}
