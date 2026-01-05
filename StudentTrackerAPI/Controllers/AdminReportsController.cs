using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AdminReportsController : ControllerBase
    {
        private readonly IAdminReportsService _service;

        public AdminReportsController(IAdminReportsService service)
        {
            _service = service;
        }

        [HttpGet("excessive-absences")]
        public async Task<IActionResult> GetStudentsWithExcessiveAbsences()
        {
            var result = await _service.GetStudentsWithExcessiveAbsencesAsync();
            return Ok(result);
        }

        [HttpGet("failing-students")]
        public async Task<IActionResult> GetFailingStudents()
        {
            var result = await _service.GetFailingStudentsAsync();
            return Ok(result);
        }

        [HttpGet("excellent-students")]
        public async Task<IActionResult> GetExcellentStudents()
        {
            var result = await _service.GetExcellentStudentsAsync();
            return Ok(result);
        }
    }
}
