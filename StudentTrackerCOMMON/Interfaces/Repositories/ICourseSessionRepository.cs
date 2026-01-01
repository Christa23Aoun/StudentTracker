using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ICourseSessionRepository
    {
        Task<int> CreateAsync(CourseSession session);
        Task<CourseSession?> GetByIdAsync(int sessionId);
        Task<IEnumerable<CourseSession>> GetByCourseAsync(int courseId);
        Task<IEnumerable<TeacherSessionConflict>> GetTeacherConflictsAsync(
            int teacherId,
            DateTime sessionDate,
            TimeSpan startTime,
            TimeSpan endTime);
        Task<IEnumerable<TeacherScheduleItemDto>> GetTeacherWeeklyScheduleAsync(
            int teacherId,
            DateTime weekStart,
            DateTime weekEnd);
        Task<int> UpdateAsync(CourseSession session);
        Task<int> DeleteAsync(int sessionId);
    }
}
