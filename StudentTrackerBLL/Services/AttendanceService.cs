using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Interfaces.Repositories;

namespace StudentTrackerBLL.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseSessionRepository _sessionRepository;

        private static readonly SemaphoreSlim _createLock = new SemaphoreSlim(1, 1);

        public AttendanceService(
            IAttendanceRepository repository,
            INotificationService notificationService,
            ICourseRepository courseRepository,
            ICourseSessionRepository sessionRepository)
        {
            _repository = repository;
            _notificationService = notificationService;
            _courseRepository = courseRepository;
            _sessionRepository = sessionRepository;
        }

        public Task<IEnumerable<Attendance>> GetBySessionIdAsync(int sessionId)
        {
            if (sessionId <= 0)
                throw new InvalidOperationException("Invalid SessionID.");

            return _repository.GetBySessionIdAsync(sessionId);
        }

        public Task<IEnumerable<int>> GetSessionIdsWithAttendanceByCourseAsync(
            int courseId,
            DateTime startDate,
            DateTime endDate)
        {
            return _repository.GetSessionIdsWithAttendanceByCourseAsync(courseId, startDate, endDate);
        }

        public async Task<int> CreateAsync(Attendance attendance)
        {
            if (attendance.StudentID <= 0 || attendance.SessionID <= 0 || attendance.CourseID <= 0)
                throw new InvalidOperationException("Invalid attendance data.");

            await _createLock.WaitAsync();
            try
            {
                var exists = await _repository.ExistsAsync(attendance.StudentID, attendance.SessionID);
                if (exists)
                    throw new InvalidOperationException("Attendance already exists.");

                var result = await _repository.CreateAsync(attendance);

                var session = await _sessionRepository.GetByIdAsync(attendance.SessionID);
                var course = await _courseRepository.GetByIdAsync(attendance.CourseID);

                var courseName = course?.CourseName ?? "your course";
                var sessionDate = session?.SessionDate.ToString("dd/MM/yyyy") ?? "a session";

                await _notificationService.NotifyStudentAsync(
                    attendance.StudentID,
                    $"Attendance recorded for {courseName} on {sessionDate}.",
                    "ATTENDANCE",
                    $"/StudentDashboard/CourseDetails?courseId={attendance.CourseID}"
                );

                return result;
            }
            finally
            {
                _createLock.Release();
            }
        }

        public async Task<int> UpdateAsync(Attendance attendance)
        {
            var result = await _repository.UpdateAsync(attendance);

            var course = await _courseRepository.GetByIdAsync(attendance.CourseID);
            var courseName = course?.CourseName ?? "your course";

            await _notificationService.NotifyStudentAsync(
                attendance.StudentID,
                $"Your attendance record has been updated for {courseName}.",
                "ATTENDANCE",
                $"/StudentDashboard/CourseDetails?courseId={attendance.CourseID}"
            );

            return result;
        }

        public Task<int> DeleteAsync(int attendanceId)
            => _repository.DeleteAsync(attendanceId);
    }
}
