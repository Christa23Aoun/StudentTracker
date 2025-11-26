using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services;
using StudentTrackerCOMMON.Models;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
        [HttpGet("ByTest")]
        public async Task<IActionResult> GetByTest(int courseId, int testId)
        {
            var result = await _service.GetGradesByTestAsync(testId, courseId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) => Ok(await _service.GetByIdAsync(id));

        [HttpGet("byCourse/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId) =>
            Ok(await _service.GetByCourseAsync(courseId));

        [HttpPost]
        public async Task<IActionResult> Create(TestGrade grade)
        {
            if (await _service.ExistsAsync(grade.TestID, grade.StudentID))
                return Conflict("A grade already exists for this student and test.");

            var id = await _service.CreateAsync(grade);
            return Ok(id);
        }

        [HttpPut]
        public async Task<IActionResult> Update(TestGrade g)
        {
            await _service.UpdateAsync(g);
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
