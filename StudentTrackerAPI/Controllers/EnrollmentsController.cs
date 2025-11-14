using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentRepository _enrollmentRepo;

        public EnrollmentsController(IEnrollmentRepository enrollmentRepo)
        {
            _enrollmentRepo = enrollmentRepo;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetCoursesByStudent(int studentId)
        {
            var courses = await _enrollmentRepo.GetCoursesByStudentAsync(studentId);
            if (courses == null || !courses.Any())
                return NotFound("No courses found for this student.");
            return Ok(courses);
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollmentRequest model)
        {
            if (model == null || model.StudentID <= 0 || model.CourseID <= 0)
                return BadRequest("Invalid data.");

            var result = await _enrollmentRepo.EnrollAsync(model.StudentID, model.CourseID);

            if (result > 0)
                return Ok(new { Message = "Student enrolled successfully." });

            return BadRequest("Student is already enrolled or enrollment failed.");
        }

        [HttpDelete("unenroll")]
        public async Task<IActionResult> UnenrollStudent([FromBody] EnrollmentRequest model)
        {
            if (model == null || model.StudentID <= 0 || model.CourseID <= 0)
                return BadRequest("Invalid data.");

            var result = await _enrollmentRepo.UnenrollAsync(model.StudentID, model.CourseID);

            if (result > 0)
                return Ok(new { Message = "Student unenrolled successfully." });

            return NotFound("Enrollment not found or already removed.");
        }
    }

    public class EnrollmentRequest
    {
        public int StudentID { get; set; }
        public int CourseID { get; set; }
    }
}
