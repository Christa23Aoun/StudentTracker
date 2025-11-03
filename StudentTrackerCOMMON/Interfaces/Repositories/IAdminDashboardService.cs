using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IAdminDashboardService
    {
        public Task<AdminDashboardDto> GetAdminDashboardAsync();
    }
}
