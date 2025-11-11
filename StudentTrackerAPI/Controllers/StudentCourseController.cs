using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;
using Dapper;
using Microsoft.Data.SqlClient;

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
            _connectionString = config.GetConnectionString("DefaultConnection");
            _service = new StudentCourseService(_connectionString);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentCourse model)
        {
            await _service.CreateAsync(model);
            return Ok("Student course created successfully");
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] StudentCourse model)
        {
            await _service.UpdateAsync(model);
            return Ok("Student course updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Student course deleted successfully");
        }

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT sc.StudentCourseID, sc.StudentID, sc.CourseID, u.FullName AS StudentName
                FROM StudentCourses sc
                INNER JOIN Users u ON u.UserID = sc.StudentID
                WHERE sc.CourseID = @CourseID";

            var result = await con.QueryAsync(sql, new { CourseID = courseId });
            return Ok(result);
        }
    }
}
