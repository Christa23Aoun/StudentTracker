using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentCoursesController : ControllerBase
    {
        private readonly StudentCourseService _service;

        public StudentCoursesController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new StudentCourseService(conn);
        }

        // ✅ Get all records
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // ✅ Get record by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // ✅ Create record
        [HttpPost]
        public async Task<IActionResult> Create(StudentCourse model)
        {
            await _service.CreateAsync(model);
            return Ok("Student course created successfully");
        }

        // ✅ Update record
        [HttpPut]
        public async Task<IActionResult> Update(StudentCourse model)
        {
            await _service.UpdateAsync(model);
            return Ok("Student course updated successfully");
        }

        // ✅ Delete record
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Student course deleted successfully");
        }

        // ✅ Get all courses for a student (Dashboard)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var data = await _service.GetCoursesByStudentAsync(userId);
            return Ok(data);
        }

        // ✅ Enroll student in a course
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromQuery] int userId, [FromQuery] int courseId)
        {
            var success = await _service.EnrollAsync(userId, courseId);
            if (!success)
                return BadRequest("Enrollment failed (already enrolled or invalid IDs)");

            return Ok("Student enrolled successfully");
        }
    }
}
