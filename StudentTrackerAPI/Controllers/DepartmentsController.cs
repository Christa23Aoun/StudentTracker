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
        private readonly ICourseRepository _courseRepo;

        public DepartmentsController(IDepartmentRepository departmentRepo, ICourseRepository courseRepo)
        {
            _departmentRepo = departmentRepo;
            _courseRepo = courseRepo;
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dep = await _departmentRepo.GetByIdAsync(id);
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
                dep.CourseCount,
                Courses = depCourses
            });
        }


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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department department)
        {
            if (department == null || id != department.DepartmentID)
                return BadRequest("Department ID mismatch.");

            var rows = await _departmentRepo.UpdateAsync(
                department.DepartmentID,
                department.DepartmentName,
                department.IsActive
            );

            if (rows <= 0)
                return NotFound("Department not found or update failed.");

            return Ok(new { Message = "Department updated successfully." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _departmentRepo.DeleteAsync(id);
            if (rows <= 0)
                return NotFound("Department not found or delete failed.");

            return Ok(new { Message = "Department deleted successfully." });
        }
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var rows = await _departmentRepo.UpdateStatusAsync(id, false);
            if (rows <= 0) return NotFound();
            return Ok(new { Message = "Department deactivated." });
        }

        [HttpPut("{id}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var rows = await _departmentRepo.UpdateStatusAsync(id, true);
            if (rows <= 0) return NotFound();
            return Ok(new { Message = "Department reactivated." });
        }

    }
}
