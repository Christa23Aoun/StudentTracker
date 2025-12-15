using System;
using System.Collections.Generic;
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

        public AttendanceService(
            IAttendanceRepository repository,
            INotificationService notificationService)
        {
            _repository = repository;
            _notificationService = notificationService;
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
            if (courseId <= 0)
                throw new InvalidOperationException("Invalid CourseID.");

            if (startDate.Date > endDate.Date)
                throw new InvalidOperationException("Invalid date range.");

            return _repository.GetSessionIdsWithAttendanceByCourseAsync(
                courseId,
                startDate,
                endDate);
        }

        public async Task<int> CreateAsync(Attendance attendance)
        {
            if (attendance == null)
                throw new InvalidOperationException("Attendance payload is missing.");

            var exists = await _repository.ExistsAsync(attendance.StudentID, attendance.SessionID);
            if (exists)
                throw new InvalidOperationException("Attendance already exists.");

            var result = await _repository.CreateAsync(attendance);

            await _notificationService.NotifyStudentAsync(
                attendance.StudentID,
                "Attendance has been recorded for one of your sessions.",
                "ATTENDANCE",
                "/Attendance"
            );

            return result;
        }

        public async Task<int> UpdateAsync(Attendance attendance)
        {
            var result = await _repository.UpdateAsync(attendance);

            await _notificationService.NotifyStudentAsync(
                attendance.StudentID,
                "Your attendance record has been updated.",
                "ATTENDANCE",
                "/Attendance"
            );

            return result;
        }

        public Task<int> DeleteAsync(int attendanceId)
        {
            return _repository.DeleteAsync(attendanceId);
        }
    }
}
