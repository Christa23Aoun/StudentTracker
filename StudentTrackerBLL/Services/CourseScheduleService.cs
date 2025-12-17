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
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly INotificationService _notificationService;

        public CourseScheduleService(
            ICourseScheduleRepository repo,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            INotificationService notificationService)
        {
            _repo = repo;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _notificationService = notificationService;
        }

        public async Task<bool> CreateScheduleAsync(CourseSchedule schedule)
        {
            var hasTeacherConflict = await _repo.CheckTeacherConflictAsync(
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                schedule.ScheduleID
            );

            if (hasTeacherConflict)
                return false;

            return await _repo.CreateAsync(schedule) > 0;
        }

        public async Task<bool> UpdateScheduleAsync(CourseSchedule schedule)
        {
            var oldSchedule = await _repo.GetByIdAsync(schedule.ScheduleID);
            if (oldSchedule == null)
                return false;

            var hasTeacherConflict = await _repo.CheckTeacherConflictAsync(
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                schedule.ScheduleID
            );

            if (hasTeacherConflict)
                return false;

            await _repo.UpdateAsync(schedule);

            var changes = new List<string>();

            if (oldSchedule.DayOfWeek != schedule.DayOfWeek)
                changes.Add($"day from {((DayOfWeek)oldSchedule.DayOfWeek)} to {((DayOfWeek)schedule.DayOfWeek)}");

            if (oldSchedule.StartTime != schedule.StartTime || oldSchedule.EndTime != schedule.EndTime)
                changes.Add($"time from {oldSchedule.StartTime:hh\\:mm}-{oldSchedule.EndTime:hh\\:mm} to {schedule.StartTime:hh\\:mm}-{schedule.EndTime:hh\\:mm}");

            if (changes.Count == 0)
                changes.Add("details were reviewed");

            var course = await _courseRepository.GetByIdAsync(schedule.CourseID);
            var courseName = course?.CourseName ?? "your course";

            var message = $"Schedule updated ({string.Join(" and ", changes)}) for {courseName}.";

            var students = await _enrollmentRepository.GetStudentsByCourseAsync(schedule.CourseID);

            foreach (var student in students)
            {
                await _notificationService.NotifyStudentAsync(
                    student.UserID,
                    message,
                    "ADMIN",
                    $"/StudentDashboard/CourseDetails?courseId={schedule.CourseID}"
                );
            }

            return true;
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

        public Task<bool> HasTeacherConflictAsync(int courseId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? ignoreScheduleId = null)
        {
            return _repo.CheckTeacherConflictAsync(courseId, dayOfWeek, startTime, endTime, ignoreScheduleId);
        }

        public Task<bool> HasStudentConflictAsync(int studentId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            return _repo.CheckStudentConflictAsync(studentId, dayOfWeek, startTime, endTime);
        }
    }
}
