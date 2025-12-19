using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services.Dashboard;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly TeacherDashboardService _dashboardService;

        public TeacherDashboardController(TeacherDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetDashboardByUser(int userId)
        {
            var dto = await _dashboardService.GetDashboardAsync(userId);
            return Ok(dto);
        }
    }
}
