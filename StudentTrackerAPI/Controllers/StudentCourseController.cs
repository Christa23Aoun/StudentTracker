using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentCoursesController : ControllerBase
    {
        private readonly StudentCourseService _service;

        public StudentCoursesController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new StudentCourseService(conn);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentCourse model)
        {
            await _service.CreateAsync(model);
            return Ok("Student course created successfully");
        }

        [HttpPut]
        public async Task<IActionResult> Update(StudentCourse model)
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
    }
}
