using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Notification notification)
        {
            var id = await _service.CreateAsync(notification);
            return Ok(new { Message = "Notification created", NotificationID = id });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var list = await _service.GetForUserAsync(userId);
            return Ok(list);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var ok = await _service.MarkAsReadAsync(id);
            return ok ? Ok("Marked as read") : NotFound();
        }

        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(count);
        }


    }
}
