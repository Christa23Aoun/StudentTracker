using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpGet("bySession/{sessionId}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var list = await _service.GetBySessionIdAsync(sessionId);
            return Ok(list);
        }

        [HttpGet("sessionIdsByCourse/{courseId}")]
        public async Task<IActionResult> GetSessionIdsByCourse(int courseId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var ids = await _service.GetSessionIdsWithAttendanceByCourseAsync(courseId, startDate, endDate);
            return Ok(ids);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attendance attendance)
        {
            try
            {
                await _service.CreateAsync(attendance);
                return Ok(new { message = "Attendance created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Attendance attendance)
        {
            await _service.UpdateAsync(attendance);
            return Ok(new { message = "Attendance updated successfully" });
        }

        [HttpDelete("{attendanceId}")]
        public async Task<IActionResult> Delete(int attendanceId)
        {
            await _service.DeleteAsync(attendanceId);
            return Ok(new { message = "Attendance deleted successfully" });
        }
    }
}
