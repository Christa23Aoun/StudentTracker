using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface ICourseScheduleService
    {
        Task<bool> CreateScheduleAsync(CourseSchedule schedule);
        Task<bool> UpdateScheduleAsync(CourseSchedule schedule);
        Task<bool> DeleteScheduleAsync(int scheduleId);
        Task<CourseSchedule?> GetByIdAsync(int scheduleId);
        Task<IEnumerable<CourseSchedule>> GetByCourseAsync(int courseId);
        Task<bool> HasTeacherConflictAsync(int courseId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? ignoreScheduleId = null);
        Task<bool> HasStudentConflictAsync(int studentId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    }
}
