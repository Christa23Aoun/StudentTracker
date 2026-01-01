using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface ITeacherDashboardService
    {
        Task<TeacherDashboardDto> GetDashboardAsync(int userId);
        Task<IEnumerable<TeacherScheduleItemDto>> GetTeacherWeeklyScheduleAsync(
            int teacherId,
            DateTime weekStart,
            DateTime weekEnd);
    }
}
