using StudentTrackerCOMMON.DTOs.AdminReports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IAdminReportsService
    {
        Task<List<ExcessiveAbsenceReportDto>> GetStudentsWithExcessiveAbsencesAsync();
        Task<List<FailingStudentReportDto>> GetFailingStudentsAsync();
        Task<List<ExcellentStudentReportDto>> GetExcellentStudentsAsync();
    }
}
