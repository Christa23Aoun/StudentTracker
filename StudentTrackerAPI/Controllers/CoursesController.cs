using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ITestGradeRepository _grades;

        public CoursesController(
            ICourseService courseService,
            ITestGradeRepository grades)
        {
            _courseService = courseService;
            _grades = grades;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            var id = await _courseService.CreateAsync(dto);
            return Ok(new { CourseID = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            // ✅ FORCE CourseID from route (fixes init-only binding issue)
            dto = dto with { CourseID = id };

            var rows = await _courseService.UpdateAsync(dto);

            if (rows <= 0)
                return BadRequest("Update failed.");

            return Ok();
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var success = await _courseService.DeactivateAsync(id);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteAsync(id);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _courseService.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("ByTeacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
        {
            var courses = await _courseService.GetByTeacherIdAsync(teacherId);
            return Ok(courses);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _courseService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetCourseStats(int id)
        {
            var stats = await _grades.GetCourseStatsAsync(id);
            return Ok(stats);
        }
    }
}
