using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseScheduleController : ControllerBase
    {
        private readonly ICourseScheduleService _scheduleService;

        public CourseScheduleController(ICourseScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var result = await _scheduleService.GetByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetById(int scheduleId)
        {
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseSchedule schedule)
        {
            var success = await _scheduleService.CreateScheduleAsync(schedule);

            if (!success)
                return BadRequest("Teacher conflict detected or schedule invalid.");

            return Ok("Schedule created successfully.");
        }

        [HttpPut("{scheduleId}")]
        public async Task<IActionResult> Update(int scheduleId, [FromBody] CourseSchedule schedule)
        {
            schedule.ScheduleID = scheduleId;

            var success = await _scheduleService.UpdateScheduleAsync(schedule);

            if (!success)
                return BadRequest("Teacher conflict detected or schedule invalid.");

            return Ok("Schedule updated successfully.");
        }

        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> Delete(int scheduleId)
        {
            var success = await _scheduleService.DeleteScheduleAsync(scheduleId);

            if (!success)
                return BadRequest("Schedule delete failed.");

            return Ok("Schedule deleted successfully.");
        }
    }
}
