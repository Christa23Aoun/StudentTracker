using System.Threading.Tasks;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardDTO> GetStudentDashboardAsync(int studentId);
    }
}
