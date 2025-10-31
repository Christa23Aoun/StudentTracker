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

        // ✅ Inject repository via constructor
        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // ✅ Get all users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users);
        }

        // ✅ Get user by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var users = await _userRepo.GetAllAsync(); // or GetByIdAsync if added later
            var user = users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // ✅ Create new user
        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            var newId = await _userRepo.CreateAsync(user);
            return Ok(new { UserID = newId });
        }

        // ✅ Update user
        [HttpPut("update")]
        public async Task<IActionResult> Update(User user)
        {
            var updated = await _userRepo.UpdateAsync(user);
            if (!updated) return BadRequest("Failed to update user");
            return Ok();
        }

        // ✅ Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userRepo.DeleteAsync(id);

            if (deleted)
                return Ok(new { message = $"✅ User with ID {id} deleted successfully" });

            return NotFound(new { message = $"⚠️ User with ID {id} not found or could not be deleted" });
        }


    }
}
