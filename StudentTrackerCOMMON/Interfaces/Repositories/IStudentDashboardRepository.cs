using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IStudentDashboardRepository
    {
        Task<StudentOverviewDTO?> GetOverviewAsync(int studentId);
        Task<IEnumerable<CourseItemDTO>> GetCoursesAsync(int studentId);
        Task<IEnumerable<NotificationDTO>> GetNotificationsAsync(int studentId);
        Task<IEnumerable<GradePointDTO>> GetGradeProgressAsync(int studentId);
        Task<IEnumerable<AttendancePointDTO>> GetAttendanceTrendAsync(int studentId);
    }
}
