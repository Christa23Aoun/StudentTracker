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
            var hasTeacherConflict = await _repo.CheckTeacherConflictAsync(
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                schedule.ScheduleID
            );

            if (hasTeacherConflict)
                return false;

            var updated = await _repo.UpdateAsync(schedule) > 0;
            if (!updated)
                return false;

            var students = await _enrollmentRepository.GetStudentsByCourseAsync(schedule.CourseID);
            var course = await _courseRepository.GetByIdAsync(schedule.CourseID);
            var courseName = course?.CourseName ?? "your course";

            foreach (var student in students)
            {
                await _notificationService.NotifyStudentAsync(
                    student.UserID,
                    $"The schedule for {courseName} has been updated.",
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
