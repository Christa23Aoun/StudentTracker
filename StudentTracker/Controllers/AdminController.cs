using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories; 

namespace StudentTracker.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminDashboardService _service; 

        public AdminController(IAdminDashboardService service) 
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
