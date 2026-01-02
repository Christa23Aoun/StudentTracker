using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly TestService _service;

        public TestsController(TestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(await _service.GetByCourseIdAsync(courseId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var test = await _service.GetByIdAsync(id);
            if (test == null)
                return NotFound();

            return Ok(test);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Test t)
        {
            try
            {
                var id = await _service.CreateAsync(t);
                return Ok(new { message = "Test created successfully", testId = id });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Test deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
