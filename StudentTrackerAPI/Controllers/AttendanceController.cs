using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _service;

        public AttendanceController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new AttendanceService(conn);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var list = await _service.GetByCourseIdAsync(courseId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attendance a)
        {
            await _service.CreateAsync(a);
            return Ok(new { message = "Attendance record created successfully" });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Attendance a)
        {
            await _service.UpdateAsync(a);
            return Ok(new { message = "Attendance record updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Attendance record deleted successfully" });
        }
    }
}
