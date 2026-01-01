using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services.Dashboard;
using StudentTrackerCOMMON.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/Dashboard/Teacher")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly TeacherDashboardService _dashboardService;
        private readonly ICourseSessionService _courseSessionService;

        public TeacherDashboardController(
            TeacherDashboardService dashboardService,
            ICourseSessionService courseSessionService)
        {
            _dashboardService = dashboardService;
            _courseSessionService = courseSessionService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDashboard(int userId)
        {
            var dto = await _dashboardService.GetDashboardAsync(userId);
            return Ok(dto);
        }

        [HttpGet("{teacherId}/schedule")]
        public async Task<IActionResult> GetTeacherSchedule(
            int teacherId,
            DateTime weekStart,
            DateTime weekEnd)
        {
            var data = await _courseSessionService.GetTeacherWeeklyScheduleAsync(
                teacherId,
                weekStart,
                weekEnd
            );

            return Ok(data);
        }
    }
}
