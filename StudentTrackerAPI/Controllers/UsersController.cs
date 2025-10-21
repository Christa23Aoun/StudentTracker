using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // POST: api/Users/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            // ✅ Adjusted to match repository signature
            var id = await _userRepository.CreateAsync(
                user.FullName,
                user.Email,
                user.PasswordHash,
                user.RoleID
            );

            return Ok(new { Message = "User created successfully", UserID = id });
        }

        // GET: api/Users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        // POST: api/Users/{id}/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var ok = await _userRepository.ActivateAsync(id);
            if (!ok) return NotFound("User not found or already active");
            return Ok("User activated successfully");
        }

        // POST: api/Users/{id}/role/{roleId}
        [HttpPost("{id}/role/{roleId}")]
        public async Task<IActionResult> SetRole(int id, int roleId)
        {
            var ok = await _userRepository.SetRoleAsync(id, roleId);
            if (!ok) return NotFound("User not found or role invalid");
            return Ok("User role updated successfully");
        }
    }
}
