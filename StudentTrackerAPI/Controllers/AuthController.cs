using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var id = await _auth.RegisterAsync(dto);
            return Created(string.Empty, new { id, message = "User registered" });
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        bool ok = await _auth.LoginAsync(dto);
        if (!ok) return Unauthorized(new { message = "Invalid credentials or inactive user" });
        return Ok(new { message = "Login successful" });
    }
}
