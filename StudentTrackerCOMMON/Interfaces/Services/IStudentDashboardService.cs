using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardDTO> GetStudentDashboardAsync(int studentId);

        Task<IEnumerable<StudentScheduleItemDto>> GetStudentScheduleAsync(
            int studentId,
            int semesterId,
            DateTime weekStart,
            DateTime weekEnd
        );
    }
}
