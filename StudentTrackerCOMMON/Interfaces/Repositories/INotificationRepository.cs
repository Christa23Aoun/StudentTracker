using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<int> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetForUserAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<int> MarkAllAsReadAsync(int userId);
    }
}
