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

        // ======================================================
        // GET ALL GRADES
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        // ======================================================
        // GET GRADES BY TEST + COURSE (Used in UI)
        // ======================================================
        [HttpGet("ByTest")]
        public async Task<IActionResult> GetByTest(int courseId, int testId)
        {
            var result = await _service.GetGradesByTestAsync(testId, courseId);
            return Ok(result);
        }

        // ======================================================
        // GET BY GRADE ID
        // ======================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound("Grade not found.");

            return Ok(item);
        }

        // ======================================================
        // GET BY COURSE (rare usage, still useful)
        // ======================================================
        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var result = await _service.GetByCourseAsync(courseId);
            return Ok(result);
        }

        // ======================================================
        // CREATE
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Create(TestGrade grade)
        {
            // prevent duplicates
            if (await _service.ExistsAsync(grade.TestID, grade.StudentID))
                return Conflict("A grade already exists for this student and test.");

            var created = await _service.CreateAsync(grade);
            return Ok(new { message = "Grade created successfully", created });
        }

        // ======================================================
        // UPDATE
        // ======================================================
        [HttpPut]
        public async Task<IActionResult> Update(TestGrade grade)
        {
            await _service.UpdateAsync(grade);
            return Ok("Grade updated successfully");
        }

        // ======================================================
        // DELETE
        // ======================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Grade deleted successfully");
        }
    }
}
