using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentRepository _repo;

        public EnrollmentsController(IEnrollmentRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetCoursesByStudent(int studentId)
        {
            var courses = await _repo.GetCoursesByStudentAsync(studentId);
            if (courses == null || !courses.Any())
                return Ok(new List<object>());
            return Ok(courses);
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest model)
        {
            if (model == null || model.StudentID <= 0 || model.CourseID <= 0)
                return BadRequest("Invalid data.");

            var result = await _repo.EnrollAsync(model.StudentID, model.CourseID);

            if (result > 0)
                return Ok(new { Message = "Enrolled successfully." });

            return BadRequest("Student is already enrolled.");
        }

        [HttpDelete("unenroll")]
        public async Task<IActionResult> Unenroll([FromBody] EnrollmentRequest model)
        {
            if (model == null || model.StudentID <= 0 || model.CourseID <= 0)
                return BadRequest("Invalid data.");

            var result = await _repo.UnenrollAsync(model.StudentID, model.CourseID);

            if (result > 0)
                return Ok(new { Message = "Unenrolled successfully." });

            return BadRequest("Enrollment not found.");
        }

        [HttpPost("reenroll")]
        public async Task<IActionResult> ReEnroll([FromBody] EnrollmentRequest model)
        {
            if (model == null || model.StudentID <= 0 || model.CourseID <= 0)
                return BadRequest("Invalid data.");

            var result = await _repo.ReEnrollAsync(model.StudentID, model.CourseID);

            if (result > 0)
                return Ok(new { Message = "Re-enrolled successfully." });

            return BadRequest("Course is already active.");
        }
    }

    public class EnrollmentRequest
    {
        public int StudentID { get; set; }
        public int CourseID { get; set; }
    }
}
