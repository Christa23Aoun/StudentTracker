using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;
        public DashboardController(DashboardService service)
        {
            _service = service;
        }


        [HttpGet("admin/summary")]
        public async Task<ActionResult> GetSummary()
        {
            var summary = await _service.GetSummaryAsync();
            return Ok(summary);
        }
        [HttpGet("admin/departments")]
        public async Task<ActionResult> GetDepartments()
        {
            var departments = await _service.GetDepartmentsAsync();
            return Ok(departments);
        }
        [HttpGet("admin/courses")]
        public async Task<ActionResult> GetCourses()
        {
            var courses = await _service.GetCoursesAsync();
            return Ok(courses);
        }

    }
}
