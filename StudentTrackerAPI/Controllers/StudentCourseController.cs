using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly IStudentCourseService _studentCourseService;

        public StudentCoursesController(IStudentCourseService studentCourseService)
        {
            _studentCourseService = studentCourseService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentCourse model)
        {
            if (model == null || model.StudentID == 0 || model.CourseID == 0)
                return BadRequest(new { message = "Invalid enrollment data." });

            var success = await _studentCourseService.EnrollAsync(model.StudentID, model.CourseID);

            if (!success)
                return BadRequest(new { message = "Enrollment failed." });

            return Ok(new { message = "Student successfully enrolled." });
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] StudentCourse model)
        {
            if (model == null || model.StudentID == 0 || model.CourseID == 0)
                return BadRequest(new { message = "Invalid enrollment data." });

            var success = await _studentCourseService.EnrollAsync(model.StudentID, model.CourseID);

            if (!success)
                return BadRequest(new { message = "Enrollment failed." });

            return Ok(new { message = "Enrollment successful." });
        }
    }
}
