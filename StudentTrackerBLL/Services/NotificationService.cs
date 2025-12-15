using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public Task<int> CreateAsync(Notification notification)
        {
            return _repo.CreateAsync(notification);
        }

        public Task<IEnumerable<Notification>> GetForUserAsync(int userId)
        {
            return _repo.GetForUserAsync(userId);
        }

        public Task<bool> MarkAsReadAsync(int notificationId)
        {
            return _repo.MarkAsReadAsync(notificationId);
        }

        public async Task NotifyStudentAsync(int studentId, string message, string type, string targetUrl)
        {
            await _repo.CreateAsync(new Notification
            {
                UserID = studentId,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task NotifyStudentsAsync(IEnumerable<int> studentIds, string message, string type, string targetUrl)
        {
            foreach (var id in studentIds.Distinct())
                await NotifyStudentAsync(id, message, type, targetUrl);
        }

        public async Task NotifyTeacherAsync(int teacherId, string message, string type, string targetUrl)
        {
            await _repo.CreateAsync(new Notification
            {
                UserID = teacherId,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task NotifyUsersAsync(IEnumerable<int> userIds, string message, string type, string targetUrl)
        {
            foreach (var id in userIds.Distinct())
            {
                await _repo.CreateAsync(new Notification
                {
                    UserID = id,
                    Message = message,
                    Type = type,
                    TargetUrl = targetUrl,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public Task<int> GetUnreadCountAsync(int userId)
            => _repo.GetUnreadCountAsync(userId);

        public Task<int> MarkAllAsReadAsync(int userId)
            => _repo.MarkAllAsReadAsync(userId);
    }
}
