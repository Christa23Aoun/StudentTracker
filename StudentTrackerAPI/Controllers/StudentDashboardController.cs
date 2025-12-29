using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/Dashboard/Student")]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IStudentDashboardService _dashboardService;

        public StudentDashboardController(IStudentDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentDashboardDTO>> Get(int studentId)
        {
            var dto = await _dashboardService.GetStudentDashboardAsync(studentId);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        [HttpGet("{studentId}/schedule")]
        public async Task<IActionResult> GetSchedule(
            int studentId,
            int semesterId,
            DateTime weekStart,
            DateTime weekEnd)
        {
            var data = await _dashboardService
                .GetStudentScheduleAsync(studentId, semesterId, weekStart, weekEnd);

            return Ok(data);
        }
    }
}
