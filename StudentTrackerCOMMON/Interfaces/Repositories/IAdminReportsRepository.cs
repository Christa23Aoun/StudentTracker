using StudentTrackerCOMMON.DTOs.AdminReports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IAdminReportsRepository
    {
        Task<List<ExcessiveAbsenceReportDto>> GetStudentsWithExcessiveAbsencesAsync(int absenceThreshold);
        Task<List<FailingStudentReportDto>> GetFailingStudentsAsync(decimal maxGrade);
        Task<List<ExcellentStudentReportDto>> GetExcellentStudentsAsync(decimal minGrade);
    }
}
