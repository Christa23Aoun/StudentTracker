using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ICourseScheduleRepository
    {
        Task<int> CreateAsync(CourseSchedule schedule);
        Task<int> UpdateAsync(CourseSchedule schedule);
        Task<int> DeleteAsync(int scheduleId);
        Task<CourseSchedule?> GetByIdAsync(int scheduleId);
        Task<IEnumerable<CourseSchedule>> GetByCourseAsync(int courseId);
        Task<bool> CheckTeacherConflictAsync(int courseId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? scheduleId = null);
        Task<bool> CheckStudentConflictAsync(int studentId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    }
}
