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

        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            var id = await _userRepository.CreateAsync(user);
            return Ok(new { Message = "User created successfully", UserID = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");
            return Ok(user);
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var ok = await _userRepository.ActivateAsync(id);
            if (!ok)
                return NotFound("User not found or already active");
            return Ok("User activated successfully");
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            var ok = await _userRepository.UpdateAsync(user);
            if (!ok)
                return NotFound("User not found.");

            return Ok("User updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _userRepository.DeleteAsync(id);
            if (!ok)
                return NotFound("User not found or already deleted.");
            return Ok("User deleted successfully.");
        }

        [HttpPost("{id}/role/{roleId}")]
        public async Task<IActionResult> SetRole(int id, int roleId)
        {
            var ok = await _userRepository.SetRoleAsync(id, roleId);
            if (!ok)
                return NotFound("User not found or role invalid");
            return Ok("User role updated successfully");
        }
    }
}
