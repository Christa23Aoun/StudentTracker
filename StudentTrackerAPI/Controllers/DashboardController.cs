using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services.Dashboard;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AdminDashboardService _service;

        public DashboardController(AdminDashboardService service)
        {
            _service = service;
        }

        [HttpGet("admin/summary")]
        public async Task<ActionResult> GetSummary()
        {
            // ✅ change this line
            var dashboard = await _service.GetAdminDashboardAsync();
            return Ok(dashboard);
        }
    }
}
