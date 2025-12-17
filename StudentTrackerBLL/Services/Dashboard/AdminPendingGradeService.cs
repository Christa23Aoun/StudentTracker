using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class AdminPendingGradeService
    {
        private readonly IAdminDashboardRepository _repo;

        public AdminPendingGradeService(IAdminDashboardRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync()
        {
            return _repo.GetPendingGradesAsync();
        }

        public Task<bool> ValidateGradeAsync(int testGradeId)
        {
            return _repo.ValidateGradeAsync(testGradeId);
        }

        public Task<bool> RejectGradeAsync(int testGradeId)
        {
            return _repo.RejectGradeAsync(testGradeId);
        }
        public async Task ValidateAllPendingAsync()
        {
            await _repo.ValidateAllPendingAsync();
        }

    }
}
