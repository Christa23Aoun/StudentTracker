using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminService;

        public DashboardController(IAdminDashboardService adminService)
        {
            _adminService = adminService;
        }

        
        [HttpGet("Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var dashboard = await _adminService.GetAdminDashboardAsync();
            return Ok(dashboard);
        }
    }
}
