using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;
    public CoursesController(ICourseService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Course created" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
    {
        if (id != dto.CourseID) return BadRequest("ID mismatch");
        await _service.UpdateAsync(dto);
        return Ok(new { id, message = "Course updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { id, message = "Course deleted" });
    }
    // ✅ NEW ENDPOINT: Get courses by teacher
    [HttpGet("byTeacher/{teacherId:int}")]
    public async Task<IActionResult> GetByTeacher(int teacherId)
    {
        var items = await _service.GetByTeacherIdAsync(teacherId);
        return Ok(items);
    }
    // ✅ NEW: course statistics for a teacher
    [HttpGet("stats/{teacherId:int}")]
    public async Task<IActionResult> GetCourseStats(int teacherId)
    {
        var stats = await _service.GetCourseStatsByTeacherAsync(teacherId);
        return Ok(stats);
    }

}
