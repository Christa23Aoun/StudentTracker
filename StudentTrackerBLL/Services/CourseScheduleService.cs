using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class CourseScheduleService : ICourseScheduleService
    {
        private readonly ICourseScheduleRepository _repo;

        public CourseScheduleService(ICourseScheduleRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> CreateScheduleAsync(CourseSchedule schedule)
        {
            var hasTeacherConflict = await HasTeacherConflictAsync(
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime
            );

            if (hasTeacherConflict)
                return false;

            return await _repo.CreateAsync(schedule) > 0;
        }

        public async Task<bool> UpdateScheduleAsync(CourseSchedule schedule)
        {
            var hasTeacherConflict = await HasTeacherConflictAsync(
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime
            );

            if (hasTeacherConflict)
                return false;

            return await _repo.UpdateAsync(schedule) > 0;
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            return await _repo.DeleteAsync(scheduleId) > 0;
        }

        public Task<CourseSchedule?> GetByIdAsync(int scheduleId)
        {
            return _repo.GetByIdAsync(scheduleId);
        }

        public Task<IEnumerable<CourseSchedule>> GetByCourseAsync(int courseId)
        {
            return _repo.GetByCourseAsync(courseId);
        }

        public Task<bool> HasTeacherConflictAsync(int courseId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            return _repo.CheckTeacherConflictAsync(courseId, dayOfWeek, startTime, endTime);
        }

        public Task<bool> HasStudentConflictAsync(int studentId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            return _repo.CheckStudentConflictAsync(studentId, dayOfWeek, startTime, endTime);
        }
    }
}
