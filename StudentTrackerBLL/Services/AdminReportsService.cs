using StudentTrackerCOMMON.DTOs.AdminReports;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class AdminReportsService : IAdminReportsService
    {
        private readonly IAdminReportsRepository _repository;

        public AdminReportsService(IAdminReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ExcessiveAbsenceReportDto>> GetStudentsWithExcessiveAbsencesAsync()
        {
            return await _repository.GetStudentsWithExcessiveAbsencesAsync(12);
        }

        public async Task<List<FailingStudentReportDto>> GetFailingStudentsAsync()
        {
            return await _repository.GetFailingStudentsAsync(50);
        }

        public async Task<List<ExcellentStudentReportDto>> GetExcellentStudentsAsync()
        {
            return await _repository.GetExcellentStudentsAsync(95);
        }
    }
}
