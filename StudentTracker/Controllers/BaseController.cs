using Microsoft.AspNetCore.Mvc;

namespace StudentTracker.Controllers
{
    public class BaseController : Controller
    {
        protected bool CheckAdminSession()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Session lost → redirect to login
                HttpContext.Response.Redirect("/Auth/Login");
                return false;
            }
            return true;
        }
    }
}
