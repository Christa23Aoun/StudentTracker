using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Password is required.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            var newId = await _userRepo.CreateAsync(user);
            return Ok(new { UserID = newId });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var updated = await _userRepo.UpdateAsync(user);
            if (!updated)
                return BadRequest("Failed to update user");

            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userRepo.HardDeleteAsync(id);

            if (deleted)
                return Ok(new { message = $"User {id} was permanently removed" });

            return NotFound(new { message = $"User {id} not found" });
        }


        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var users = await _userRepo.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return NotFound(new { message = $"No user found with email {email}" });

            return Ok(user);
        }
    }
}
