using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentCourseService _studentCourseService;

        public EnrollmentsController(IStudentCourseService studentCourseService)
        {
            _studentCourseService = studentCourseService;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetCoursesByStudent(int studentId)
        {
            var result = await _studentCourseService.GetCoursesByStudentAsync(studentId);
            return Ok(result ?? new List<StudentCourse>());
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest dto)
        {
            if (dto == null || dto.StudentID <= 0 || dto.CourseID <= 0)
                return BadRequest("Invalid enrollment request.");

            var success = await _studentCourseService.EnrollAsync(dto.StudentID, dto.CourseID);

            return success
                ? Ok(new { message = "Enrolled successfully." })
                : BadRequest(new { message = "Enrollment failed." });
        }

        [HttpDelete("unenroll")]
        public async Task<IActionResult> Unenroll([FromBody] EnrollmentRequest dto)
        {
            if (dto == null || dto.StudentID <= 0 || dto.CourseID <= 0)
                return BadRequest("Invalid unenroll request.");

            var rows = await _studentCourseService.UnenrollAsync(dto.StudentID, dto.CourseID);

            return rows > 0
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
