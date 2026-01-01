using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseSessionsController : ControllerBase
    {
        private readonly ICourseSessionService _service;

        public CourseSessionsController(ICourseSessionService service)
        {
            _service = service;
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var list = await _service.GetByCourseAsync(courseId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> AddSession([FromBody] CourseSession session)
        {
            var success = await _service.AddSessionAsync(session);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSessions([FromBody] GenerateSessionsRequest req)
        {
            var success = await _service.GenerateSessionsAsync(req);
            return success ? Ok() : BadRequest();
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> Delete(int sessionId)
        {
            var success = await _service.DeleteSessionAsync(sessionId);
            return success ? Ok() : BadRequest();
        }
    }
}
