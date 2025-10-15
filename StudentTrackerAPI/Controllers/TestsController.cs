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

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));
        [HttpPost] public async Task<IActionResult> Create(Test t) { await _service.CreateAsync(t); return Ok("Test created successfully"); }
        [HttpPut] public async Task<IActionResult> Update(Test t) { await _service.UpdateAsync(t); return Ok("Test updated successfully"); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return Ok("Test deleted successfully"); }
    }
}
