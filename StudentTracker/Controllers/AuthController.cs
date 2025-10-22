using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;

namespace StudentTracker.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet] public IActionResult Login() => View();
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // placeholder; will use Identity later
            return RedirectToAction("Index", "Users");
        }

        [HttpGet] public IActionResult Register() => View();
        [HttpPost]
        public IActionResult Register(User user)
        {
            // placeholder; will call Identity later
            return RedirectToAction("Login");
        }
    }
}
