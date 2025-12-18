using StudentTrackerCOMMON.DTOs.AdminDashboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IAdminDashboardRepository
    {
        Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync();
        Task<bool> ValidateGradeAsync(int testGradeId);
        Task<bool> RejectGradeAsync(int testGradeId);
        Task ValidateAllPendingAsync();
        Task<int> CountPendingGradesAsync();

    }
}
