using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Password is required.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            var newId = await _userService.CreateAsync(user);
            return Ok(new { UserID = newId });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var updated = await _userService.UpdateAsync(user);
            if (!updated)
                return BadRequest("Failed to update user");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return Ok();
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
