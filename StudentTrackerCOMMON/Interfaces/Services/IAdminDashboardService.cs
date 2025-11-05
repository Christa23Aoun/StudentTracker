using System.Threading.Tasks;
using StudentTrackerCOMMON.DTOs.AdminDashboard;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
    }
}
