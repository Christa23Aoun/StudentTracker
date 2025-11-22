using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly TestService _service;

        public TestsController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new TestService(conn);
        }

        // GET /api/Tests
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        // GET /api/Tests/byCourse/5
        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId) =>
            Ok(await _service.GetByCourseIdAsync(courseId));

        // GET /api/Tests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) =>
            Ok(await _service.GetByIdAsync(id));

        // POST /api/Tests
        [HttpPost]
        public async Task<IActionResult> Create(Test t)
        {
            try
            {
                await _service.CreateAsync(t);
                return Ok(new { message = "Test created successfully" });
            }
            catch (ArgumentException ex)
            {
                // validation error (weight > 100)
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // unexpected error
                return StatusCode(500, "Server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(Test t)
        {
            try
            {
                await _service.UpdateAsync(t);
                return Ok(new { message = "Test updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Server error");
            }
        }


        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Test deleted successfully" });
        }
    }
}
