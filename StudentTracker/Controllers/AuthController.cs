using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;

        public AuthController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("API");
        }

        // ---------- LOGIN ----------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            Console.WriteLine($"🔹 Login attempt: {email} | role = {role}");

            var dto = new LoginDto { Email = email, Password = password };

            try
            {
                var response = await _http.PostAsJsonAsync("api/Auth/login", dto);
                Console.WriteLine($"🔹 API login status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var userResponse = await _http.GetAsync($"api/Users/email/{email}");
                    Console.WriteLine($"🔹 User lookup status: {userResponse.StatusCode}");

                    if (userResponse.IsSuccessStatusCode)
                    {
                        var user = await userResponse.Content.ReadFromJsonAsync<StudentTrackerCOMMON.Models.User>();
                        Console.WriteLine($"✅ Logged in user: {user?.FullName} ({user?.Email}) role={user?.RoleID}");

                        HttpContext.Session.SetString("UserName", user.FullName);
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetInt32("UserRole", user.RoleID);
                        HttpContext.Session.SetInt32("UserID", user.UserID);
                    }

                    // ✅ Redirect based on role
                    if (role == "Admin")
                    {
                        Console.WriteLine("➡ Redirecting to Admin");
                        return RedirectToAction("Index", "Admin");
                    }
                    if (role == "Teacher")
                    {
                        Console.WriteLine("➡ Redirecting to Teacher Dashboard");
                        return RedirectToAction("Dashboard", "Teacher");
                    }
                    if (role == "Student")
                    {
                        Console.WriteLine("➡ Redirecting to Home");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    Console.WriteLine("❌ Invalid login — bad credentials or failed API.");
                    ViewBag.Error = "Invalid email or password.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Exception: " + ex.Message);
                ViewBag.Error = "Connection error: " + ex.Message;
            }

            ViewBag.Role = role;
            return View();
        }



        // ---------- ROLE-SPECIFIC ENTRY POINTS ----------
        [HttpGet] public IActionResult LoginStudent() { ViewBag.Role = "Student"; return View("Login"); }
        [HttpGet] public IActionResult LoginTeacher() { ViewBag.Role = "Teacher"; return View("Login"); }
        [HttpGet] public IActionResult LoginAdmin() { ViewBag.Role = "Admin"; return View("Login"); }

        // ---------- REGISTER ----------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Auth/register", dto);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Login");

                ViewBag.Error = "Registration failed. Please try again.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(dto);
        }
    }
}
