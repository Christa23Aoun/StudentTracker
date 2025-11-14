using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepo;
        private readonly ICourseRepository _courseRepo; // 🔹 add this

        public DepartmentsController(IDepartmentRepository departmentRepo, ICourseRepository courseRepo)
        {
            _departmentRepo = departmentRepo;
            _courseRepo = courseRepo;
        }

        // ✅ GET: api/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentRepo.GetAllAsync();
            var courses = await _courseRepo.GetAllAsync();

            var result = departments.Select(d => new
            {
                d.DepartmentID,
                d.DepartmentName,
                d.CreatedAt,
                d.IsActive,
                CourseCount = courses.Count(c => c.DepartmentID == d.DepartmentID),
                Courses = courses
                    .Where(c => c.DepartmentID == d.DepartmentID)
                    .Select(c => new { c.CourseID, c.CourseName })
                    .ToList()
            });

            return Ok(result);
        }

        // ✅ GET: api/departments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var departments = await _departmentRepo.GetAllAsync();
            var dep = departments.FirstOrDefault(d => d.DepartmentID == id);
            if (dep == null)
                return NotFound();

            var courses = await _courseRepo.GetAllAsync();
            var depCourses = courses
                .Where(c => c.DepartmentID == id)
                .Select(c => new { c.CourseID, c.CourseName })
                .ToList();

            return Ok(new
            {
                dep.DepartmentID,
                dep.DepartmentName,
                dep.CreatedAt,
                dep.IsActive,
                CourseCount = depCourses.Count,
                Courses = depCourses
            });
        }

        // ✅ POST: api/departments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Department department)
        {
            var name = department?.DepartmentName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Department name is required.");

            var id = await _departmentRepo.CreateAsync(name);
            if (id <= 0)
                return StatusCode(500, "Failed to create department.");

            return Ok(new { DepartmentID = id, Message = "Department created successfully." });
        }

        // ✅ PUT: api/departments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department department)
        {
            if (department == null || id != department.DepartmentID)
                return BadRequest("Department ID mismatch.");

            var name = department.DepartmentName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Department name is required.");

            var rows = await _departmentRepo.UpdateAsync(id, name);
            if (rows <= 0)
                return NotFound("Department not found or update failed.");

            return Ok(new { Message = "Department updated successfully." });
        }

        // ✅ DELETE: api/departments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _departmentRepo.DeleteAsync(id);
            if (rows <= 0)
                return NotFound("Department not found or delete failed.");

            return Ok(new { Message = "Department deleted successfully." });
        }
    }
}
