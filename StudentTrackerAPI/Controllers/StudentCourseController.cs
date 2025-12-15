using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly IStudentCourseService _studentCourseService;
        private readonly IUserService _userService;

        public StudentCoursesController(
            IStudentCourseService studentCourseService,
            IUserService userService)
        {
            _studentCourseService = studentCourseService;
            _userService = userService;
        }

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest();

            var enrollments = await _studentCourseService.GetByCourseAsync(courseId);
            if (enrollments == null || !enrollments.Any())
                return Ok(new List<object>());

            var result = new List<object>();

            foreach (var e in enrollments)
            {
                var user = await _userService.GetByIdAsync(e.StudentID);
                if (user == null) continue;

                result.Add(new
                {
                    StudentID = user.UserID,
                    StudentName = user.FullName
                });
            }

            return Ok(result);
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
