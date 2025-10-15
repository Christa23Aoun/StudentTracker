using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _service;

        public AttendanceController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new AttendanceService(conn);
        }

        [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("{id}")] public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));
        [HttpPost] public async Task<IActionResult> Create(Attendance a) { await _service.CreateAsync(a); return Ok("Attendance record created successfully"); }
        [HttpPut] public async Task<IActionResult> Update(Attendance a) { await _service.UpdateAsync(a); return Ok("Attendance record updated successfully"); }
        [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id) { await _service.DeleteAsync(id); return Ok("Attendance record deleted successfully"); }
    }
}
