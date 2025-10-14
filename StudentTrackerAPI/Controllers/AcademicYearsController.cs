using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AcademicYearsController : ControllerBase
{
    private readonly IAcademicYearService _service;
    public AcademicYearsController(IAcademicYearService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AcademicYearCreateDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "AcademicYear created" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AcademicYearUpdateDto dto)
    {
        if (id != dto.AcademicYearID) return BadRequest("ID mismatch");
        await _service.UpdateAsync(dto);
        return Ok(new { id, message = "AcademicYear updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { id, message = "AcademicYear deleted" });
    }
}
