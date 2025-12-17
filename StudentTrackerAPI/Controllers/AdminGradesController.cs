using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerBLL.Services.Dashboard;


namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminGradesController : ControllerBase
    {
        private readonly AdminPendingGradeService _service;

        public AdminGradesController(AdminPendingGradeService service)
        {
            _service = service;
        }

        // ✔ GET all pending grades
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingGrades()
        {
            var result = await _service.GetPendingGradesAsync();
            return Ok(result);
        }

        // ✔ Validate a grade
        [HttpPost("validate/{id}")]
        public async Task<IActionResult> ValidateGrade(int id)
        {
            var success = await _service.ValidateGradeAsync(id);
            return success ? Ok("Validated") : BadRequest("Failed");
        }

        // ✔ Reject/Delete a grade
        [HttpDelete("reject/{id}")]
        public async Task<IActionResult> RejectGrade(int id)
        {
            var success = await _service.RejectGradeAsync(id);
            return success ? Ok("Rejected") : BadRequest("Failed");
        }
        [HttpPost("validate-all")]
        public async Task<IActionResult> ValidateAll()
        {
            await _service.ValidateAllPendingAsync();
            return Ok("All grades validated");
        }



    }
}
