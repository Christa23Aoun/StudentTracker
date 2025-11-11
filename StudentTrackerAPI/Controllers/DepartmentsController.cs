using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepo;

        public DepartmentsController(IDepartmentRepository departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }

        // GET: api/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _departmentRepo.GetAllAsync();
            return Ok(list);
        }

        // GET: api/departments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var list = await _departmentRepo.GetAllAsync();
            var dep = list.FirstOrDefault(d => d.DepartmentID == id);

            if (dep == null)
                return NotFound();

            return Ok(dep);
        }



        // POST: api/departments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Department department)
        {
            var name = department?.DepartmentName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Department name is required.");

            var id = await _departmentRepo.CreateAsync(name);
            if (id <= 0) return StatusCode(500, "Failed to create department.");

            return Ok(new { DepartmentID = id, Message = "Department created successfully." });
        }

        // PUT: api/departments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department department)
        {
            if (department == null || id != department.DepartmentID)
                return BadRequest("Department ID mismatch.");

            var name = department.DepartmentName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Department name is required.");

            var rows = await _departmentRepo.UpdateAsync(id, name);
            if (rows <= 0) return NotFound("Department not found or update failed.");

            return Ok(new { Message = "Department updated successfully." });
        }

        // DELETE: api/departments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _departmentRepo.DeleteAsync(id);
            if (rows <= 0) return NotFound("Department not found or delete failed.");

            return Ok(new { Message = "Department deleted successfully." });
        }
    }
}
