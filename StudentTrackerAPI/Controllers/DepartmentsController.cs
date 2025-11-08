using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

  
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _service.GetAllAsync();
        return Ok(departments);
    }

   
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _service.GetByIdAsync(id);
        if (department == null) return NotFound(new { message = "Department not found" });
        return Ok(department);
    }

 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newId = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId, message = "Department created successfully" });
    }

  
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
    {
        if (id != dto.DepartmentID)
            return BadRequest("Mismatched Department ID");

        var result = await _service.UpdateAsync(dto);
        if (result == 0)
            return NotFound(new { message = "Department not found or not updated" });

        return Ok(new { message = "Department updated successfully" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (result == 0)
            return NotFound(new { message = "Department not found or already deleted" });

        return Ok(new { message = "Department deleted successfully" });
    }
}
