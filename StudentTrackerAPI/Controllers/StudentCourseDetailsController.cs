using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/Dashboard/Student/{studentId}/course")]
    public class StudentCourseDetailsController : ControllerBase
    {
        private readonly IStudentCourseDetailsService _service;

        public StudentCourseDetailsController(IStudentCourseDetailsService service)
        {
            _service = service;
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> Get(int studentId, int courseId)
        {
            var dto = await _service.GetCourseDetailsAsync(studentId, courseId);
            return Ok(dto);
        }
    }
}
