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

        public DashboardController(IUserRepository users, ICourseRepository courses)
        {
            _users = users;
            _courses = courses;
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
                {
                    totalStudents++;
                }
                else if (user.RoleID == 2 && user.IsActive)
                {
                    totalActiveTeachers++;
                }
            }

            foreach (var course in courses)
            {
                if (course.IsActive)
                {
                    activeCourses++;
                }
            }

            return Ok(new
            {
                totalStudents = totalStudents,
                totalActiveTeachers = totalActiveTeachers,
                activeCoursesThisSemester = activeCourses
            });
        }
    }
}
