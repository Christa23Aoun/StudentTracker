using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestGradesController : ControllerBase
    {
        private readonly TestGradeService _service;

        public TestGradesController(IConfiguration config)
        {
            string conn = config.GetConnectionString("DefaultConnection");
            _service = new TestGradeService(conn);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("ByTest")]
        public async Task<IActionResult> GetByTest(int courseId, int testId)
        {
            var result = await _service.GetGradesByTestAsync(testId, courseId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound("Grade not found.");

            return Ok(item);
        }

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var result = await _service.GetByCourseAsync(courseId);
            return Ok(result);
        }

        // ======================================================
        // NEW ENDPOINT: Get AVERAGE for one TEST
        // ======================================================
        [HttpGet("AverageByTest/{testId}")]
        public async Task<IActionResult> GetAverageByTest(int testId)
        {
            var avg = await _service.GetAverageByTestAsync(testId);
            return Ok(avg);
        }

        // ======================================================
        // NEW ENDPOINT: Get AVERAGE for course
        // ======================================================
        [HttpGet("AverageByCourse/{courseId}")]
        public async Task<IActionResult> GetAverageByCourse(int courseId)
        {
            var avg = await _service.GetAverageByCourseAsync(courseId);
            return Ok(avg);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TestGrade grade)
        {
            if (await _service.ExistsAsync(grade.TestID, grade.StudentID))
                return Conflict("A grade already exists for this student and test.");

            var created = await _service.CreateAsync(grade);
            return Ok(new { message = "Grade created successfully", created });
        }

        [HttpPut]
        public async Task<IActionResult> Update(TestGrade grade)
        {
            await _service.UpdateAsync(grade);
            return Ok("Grade updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Grade deleted successfully");
        }
    }
}
