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

        // GET: api/Enrollments/student/1025
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetCoursesByStudent(int studentId)
        {
            var result = await _repo.GetCoursesByStudentAsync(studentId);

            if (result == null || !result.Any())
                return Ok(new List<StudentCourse>());

            return Ok(result);
        }

        // POST: api/Enrollments/enroll
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest dto)
        {
            if (dto == null || dto.StudentID <= 0 || dto.CourseID <= 0)
                return BadRequest("Invalid enrollment request.");

            var result = await _repo.EnrollAsync(dto.StudentID, dto.CourseID);

            return result > 0
                ? Ok(new { message = "Enrolled successfully." })
                : BadRequest(new { message = "Enrollment failed." });
        }

        // DELETE: api/Enrollments/unenroll
        [HttpDelete("unenroll")]
        public async Task<IActionResult> Unenroll([FromBody] EnrollmentRequest dto)
        {
            if (dto == null || dto.StudentID <= 0 || dto.CourseID <= 0)
                return BadRequest("Invalid unenroll request.");

            var result = await _repo.UnenrollAsync(dto.StudentID, dto.CourseID);

            return result > 0
                ? Ok(new { message = "Unenrolled successfully." })
                : BadRequest(new { message = "Unenroll failed." });
        }
    }

    public class EnrollmentRequest
    {
        public int StudentID { get; set; }
        public int CourseID { get; set; }
    }
}
