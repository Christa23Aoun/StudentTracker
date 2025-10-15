using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestGradesController : ControllerBase
    {
        private readonly TestGradeService _service;

        public TestGradesController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new TestGradeService(conn);
        }

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));
        [HttpPost] public async Task<IActionResult> Create(TestGrade g) { await _service.CreateAsync(g); return Ok("Grade created successfully"); }
        [HttpPut] public async Task<IActionResult> Update(TestGrade g) { await _service.UpdateAsync(g); return Ok("Grade updated successfully"); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return Ok("Grade deleted successfully"); }
    }
}
