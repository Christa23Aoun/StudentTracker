//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using StudentTracker.Models;
//using System.Text;

//namespace StudentTracker.Controllers
//{
//    public class AuthController : Controller
//    {
//        private readonly HttpClient _client;
//        private readonly string _apiBase;

//        public AuthController(IHttpClientFactory factory, IConfiguration config)
//        {
//            _client = factory.CreateClient("API");
//            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
//        }

//        // ============================
//        // LOGIN (GET)
//        // ============================
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Login(string? role = null)
//        {
//            // ❗ No default to Student anymore.
//            // If role is null → generic login (no role restriction).
//            ViewBag.Role = role;
//            ViewData["Title"] = role == null ? "Login" : $"Login - {role}";
//            return View();
//        }

//        // ============================
//        // LOGIN (POST)
//        // ============================
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginView model)
//        {
//            // Preserve selected role on error
//            ViewBag.Role = model.Role;

//            if (!ModelState.IsValid)
//                return View(model);

//            try
//            {
//                // 1️⃣ Validate credentials using API
//                var payload = JsonConvert.SerializeObject(model);
//                var content = new StringContent(payload, Encoding.UTF8, "application/json");
//                var res = await _client.PostAsync($"{_apiBase}Auth/login", content);

//                if (!res.IsSuccessStatusCode)
//                {
//                    ViewBag.Error = "Invalid email or password.";
//                    return View(model);
//                }

//                // 2️⃣ Fetch user details
//                var userRes = await _client.GetAsync($"{_apiBase}Users/email/{model.Email}");
//                if (!userRes.IsSuccessStatusCode)
//                {
//                    ViewBag.Error = "Failed to fetch user details.";
//                    return View(model);
//                }

//                var json = await userRes.Content.ReadAsStringAsync();
//                var user = JsonConvert.DeserializeObject<UserView>(json);

//                if (user == null)
//                {
//                    ViewBag.Error = "User not found.";
//                    return View(model);
//                }

//                // 3️⃣ Convert RoleID → RoleName
//                string roleName = user.RoleID switch
//                {
//                    1 => "Admin",
//                    2 => "Teacher",
//                    3 => "Student",
//                    _ => "Unknown"
//                };

//                // 4️⃣ ROLE VALIDATION
//                // - If model.Role is null/empty → generic login (no restriction).
//                // - If model.Role has value (Student/Teacher/Admin from Connect dropdown),
//                //   then enforce that it matches the DB role.
//                if (!string.IsNullOrWhiteSpace(model.Role) &&
//                    !string.Equals(roleName, model.Role, StringComparison.OrdinalIgnoreCase))
//                {
//                    string requestedRole = model.Role;
//                    ViewBag.Error = $"This account belongs to a {roleName}. You cannot log in as a {requestedRole}.";
//                    return View(model);
//                }

//                // 5️⃣ Save session user info
//                HttpContext.Session.SetString("UserName", user.FullName);
//                HttpContext.Session.SetString("UserEmail", user.Email);
//                HttpContext.Session.SetString("UserRole", roleName);
//                HttpContext.Session.SetInt32("UserID", user.UserID);
//                HttpContext.Session.SetInt32("RoleID", user.RoleID);

//                // 6️⃣ Teacher-specific info
//                if (roleName == "Teacher")
//                {
//                    try
//                    {
//                        var teacherRes = await _client.GetAsync($"{_apiBase}TeacherDashboard/byEmail/{user.Email}");
//                        if (teacherRes.IsSuccessStatusCode)
//                        {
//                            var teacherJson = await teacherRes.Content.ReadAsStringAsync();
//                            var teacher = JsonConvert.DeserializeObject<TeacherView>(teacherJson);
//                            if (teacher != null)
//                                HttpContext.Session.SetInt32("TeacherID", teacher.TeacherID);
//                        }
//                    }
//                    catch
//                    {
//                        // Silent fail – TeacherID is only extra info
//                    }
//                }

//                // 7️⃣ Authentication cookie
//                var claims = new List<Claim>
//                {
//                    new Claim(ClaimTypes.Name, user.FullName),
//                    new Claim(ClaimTypes.Email, user.Email),
//                    new Claim(ClaimTypes.Role, roleName)
//                };

//                var identity = new ClaimsIdentity(claims, "CookieAuth");
//                var principal = new ClaimsPrincipal(identity);
//                await HttpContext.SignInAsync("CookieAuth", principal);

//                // 8️⃣ Redirect based on TRUE role from DB
//                return roleName switch
//                {
//                    "Admin" => RedirectToAction("Dashboard", "Admin"),
//                    "Teacher" => RedirectToAction("Dashboard", "Teacher"),
//                    "Student" => RedirectToAction("Index", "StudentDashboard"),
//                    _ => RedirectToAction("Login")
//                };
//            }
//            catch (Exception ex)
//            {
//                ViewBag.Error = "Server connection error: " + ex.Message;
//                return View(model);
//            }
//        }

//        // ============================
//        // Role-specific login shortcuts
//        // ============================
//        [HttpGet, AllowAnonymous]
//        public IActionResult LoginStudent()
//        {
//            ViewBag.Role = "Student";
//            ViewData["Title"] = "Login - Student";
//            return View("Login");
//        }

//        [HttpGet, AllowAnonymous]
//        public IActionResult LoginTeacher()
//        {
//            ViewBag.Role = "Teacher";
//            ViewData["Title"] = "Login - Teacher";
//            return View("Login");
//        }

//        [HttpGet, AllowAnonymous]
//        public IActionResult LoginAdmin()
//        {
//            ViewBag.Role = "Admin";
//            ViewData["Title"] = "Login - Admin";
//            return View("Login");
//        }

//        // ============================
//        // REGISTER (optional)
//        // ============================
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Register() => View();

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterView model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            try
//            {
//                var payload = JsonConvert.SerializeObject(model);
//                var content = new StringContent(payload, Encoding.UTF8, "application/json");
//                var res = await _client.PostAsync($"{_apiBase}Auth/register", content);

//                if (res.IsSuccessStatusCode)
//                {
//                    TempData["Msg"] = "Registration successful! Please log in.";
//                    return RedirectToAction("Login");
//                }

//                ViewBag.Error = "Registration failed. Please try again.";
//            }
//            catch (Exception ex)
//            {
//                ViewBag.Error = "Server error: " + ex.Message;
//            }

//            return View(model);
//        }

//        // ============================
//        // LOGOUT
//        // ============================
//        [Authorize]
//        public async Task<IActionResult> Logout()
//        {
//            HttpContext.Session.Clear();
//            await HttpContext.SignOutAsync("CookieAuth");
//            return RedirectToAction("Login", "Auth");
//        }
//    }
//}
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
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? role = null)
        {
            ViewBag.Role = role; // null = generic login
            ViewData["Title"] = role == null ? "Login" : $"Login - {role}";
            return View();
        }

        // POST Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            ViewBag.Role = model.Role;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var payload = JsonConvert.SerializeObject(model);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{_apiBase}Auth/login", content);

                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View(model);
                }

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

                string roleName = user.RoleID switch
                {
                    1 => "Admin",
                    2 => "Teacher",
                    3 => "Student",
                    _ => "Unknown"
                };

                // Enforce role match ONLY if UI role is specified (Teacher/Student)
                if (!string.IsNullOrWhiteSpace(model.Role) &&
                    !string.Equals(roleName, model.Role, StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = $"This account belongs to a {roleName}. You cannot log in as a {model.Role}.";
                    return View(model);
                }

                // Save session
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", roleName);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);

                if (roleName == "Teacher")
                {
                    var teacherRes = await _client.GetAsync($"{_apiBase}TeacherDashboard/byEmail/{user.Email}");
                    if (teacherRes.IsSuccessStatusCode)
                    {
                        var teacherJson = await teacherRes.Content.ReadAsStringAsync();
                        var teacher = JsonConvert.DeserializeObject<TeacherView>(teacherJson);
                        if (teacher != null)
                            HttpContext.Session.SetInt32("TeacherID", teacher.TeacherID);
                    }
                }

                // Cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roleName)
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("CookieAuth", principal);

                // Redirect
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

        // LOGIN SHORTCUTS
        [HttpGet, AllowAnonymous]
        public IActionResult LoginStudent()
        {
            ViewBag.Role = "Student";
            ViewData["Title"] = "Login - Student";
            return View("Login");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginTeacher()
        {
            ViewBag.Role = "Teacher";
            ViewData["Title"] = "Login - Teacher";
            return View("Login");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult LoginAdmin()
        {
            ViewBag.Role = "Admin";
            ViewData["Title"] = "Login - Admin";
            return View("Login");
        }

        // LOGOUT
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login", "Auth");
        }
    }
}
