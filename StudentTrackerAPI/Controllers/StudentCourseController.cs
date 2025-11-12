using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using StudentTrackerCOMMON.Models;
using StudentTrackerBLL.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly StudentCourseService _service;
        private readonly string _connectionString;

        public StudentCoursesController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _service = new StudentCourseService(_connectionString);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentCourse model)
        {
            if (model.StudentID == 0 || model.CourseID == 0)
                return BadRequest(new { message = "Invalid IDs" });

            using var con = new SqlConnection(_connectionString);
            var role = await con.ExecuteScalarAsync<int>(
                "SELECT RoleID FROM Users WHERE UserID = @ID", new { ID = model.StudentID });

            if (role != 3)
                return BadRequest(new { message = "Only students can be enrolled." });

            await _service.CreateAsync(model);
            return Ok(new { message = "✅ Student successfully enrolled." });
        }

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    sc.StudentCourseID,
                    sc.StudentID,
                    sc.CourseID,
                    u.FullName AS StudentName
                FROM StudentCourses sc
                INNER JOIN Users u ON u.UserID = sc.StudentID
                WHERE sc.CourseID = @CourseID AND u.RoleID = 3
                ORDER BY u.FullName";
            var result = await con.QueryAsync(sql, new { CourseID = courseId });
            return Ok(result);
        }
    }
}
