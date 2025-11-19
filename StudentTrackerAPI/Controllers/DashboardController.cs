using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly ICourseRepository _courses;
        private readonly ITestGradeRepository _testGrades;

        public DashboardController(
            IUserRepository users,
            ICourseRepository courses,
            ITestGradeRepository testGrades)
        {
            _users = users;
            _courses = courses;
            _testGrades = testGrades;
        }

        [HttpGet("AdminSummary")]
        public async Task<IActionResult> GetAdminSummary()
        {
            var users = await _users.GetAllAsync();
            var courses = await _courses.GetAllAsync();

            int totalStudents = 0;
            int totalActiveTeachers = 0;
            int activeCourses = 0;

            foreach (var user in users)
            {
                if (user.RoleID == 3)
                    totalStudents++;
                else if (user.RoleID == 2 && user.IsActive)
                    totalActiveTeachers++;
            }

            foreach (var course in courses)
            {
                if (course.IsActive)
                    activeCourses++;
            }

            return Ok(new
            {
                totalStudents = totalStudents,
                totalActiveTeachers = totalActiveTeachers,
                activeCoursesThisSemester = activeCourses
            });
        }

        [HttpGet("PendingGrades")]
        public async Task<IActionResult> GetPendingGrades()
        {
            var pending = await _testGrades.GetPendingGradesAsync();
            return Ok(pending);
        }

        [HttpPost("ValidateGrade/{id}")]
        public async Task<IActionResult> ValidateGrade(int id)
        {
            var affected = await _testGrades.MarkGradeAsValidatedAsync(id);
            if (affected == 0)
                return NotFound();

            return NoContent();
        }

        [HttpPost("RejectGrade/{id}")]
        public async Task<IActionResult> RejectGrade(int id)
        {
            var affected = await _testGrades.DeleteGradeAsync(id);
            if (affected == 0)
                return NotFound();

            return NoContent();
        }
    }
}
