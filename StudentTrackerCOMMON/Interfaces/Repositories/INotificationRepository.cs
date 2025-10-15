using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<int> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetForUserAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
    }
}
