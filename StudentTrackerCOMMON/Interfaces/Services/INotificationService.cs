using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface INotificationService
    {
        Task<int> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetForUserAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);

        Task NotifyStudentAsync(
            int studentId,
            string message,
            string type,
            string targetUrl);

        Task NotifyStudentsAsync(
            IEnumerable<int> studentIds,
            string message,
            string type,
            string targetUrl);

        Task NotifyTeacherAsync(
            int teacherId,
            string message,
            string type,
            string targetUrl);

        Task NotifyUsersAsync(
            IEnumerable<int> userIds,
            string message,
            string type,
            string targetUrl);

        Task<int> GetUnreadCountAsync(int userId);
        Task<int> MarkAllAsReadAsync(int userId);
    }
}
