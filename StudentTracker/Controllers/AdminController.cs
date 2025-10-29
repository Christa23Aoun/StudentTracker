using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services.Dashboard;

namespace StudentTracker.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDashboardService _service;

        public AdminController(AdminDashboardService service)
        {
            _service = service;
        }

        [HttpGet("/admin/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var model = await _service.GetAdminDashboardAsync();
            return View("~/Views/Dashboard/Admin.cshtml", model);
        }
    }
}
