using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _repo;

        public NotificationsController(INotificationRepository repo)
        {
            _repo = repo;
        }

        
        [HttpPost("create")]
        public async Task<IActionResult> Create(Notification notification)
        {
            var id = await _repo.CreateAsync(notification);
            return Ok(new { Message = "Notification created", NotificationID = id });
        }

       
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var list = await _repo.GetForUserAsync(userId);
            return Ok(list);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var ok = await _repo.MarkAsReadAsync(id);
            return ok ? Ok("Marked as read") : NotFound();
        }
    }
}
