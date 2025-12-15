using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public Task<IEnumerable<int>> GetSessionIdsWithAttendanceByCourseAsync(int courseId, DateTime startDate, DateTime endDate)
        {
            if (courseId <= 0)
                throw new InvalidOperationException("Invalid CourseID.");

            if (startDate.Date > endDate.Date)
                throw new InvalidOperationException("Invalid date range.");

            return _repository.GetSessionIdsWithAttendanceByCourseAsync(courseId, startDate, endDate);
        }

        public async Task<int> CreateAsync(Attendance attendance)
        {
            if (attendance == null)
                throw new InvalidOperationException("Attendance payload is missing.");

            if (attendance.StudentID <= 0)
                throw new InvalidOperationException("Invalid StudentID.");

            if (attendance.SessionID <= 0)
                throw new InvalidOperationException("Invalid SessionID.");

            var exists = await _repository.ExistsAsync(attendance.StudentID, attendance.SessionID);
            if (exists)
                throw new InvalidOperationException("Attendance already exists for this student and session.");

            var result = await _repository.CreateAsync(attendance);

            await _notificationService.NotifyStudentAsync(
                attendance.StudentID,
                "Attendance has been recorded for one of your sessions.",
                "INFO"
            );

            return result;
        }

        public async Task<int> UpdateAsync(Attendance attendance)
        {
            if (attendance == null)
                throw new InvalidOperationException("Attendance payload is missing.");

            if (attendance.AttendanceID <= 0)
                throw new InvalidOperationException("Invalid AttendanceID.");

            if (attendance.StudentID <= 0)
                throw new InvalidOperationException("Invalid StudentID.");

            var result = await _repository.UpdateAsync(attendance);

            await _notificationService.NotifyStudentAsync(
                attendance.StudentID,
                "Your attendance record has been updated.",
                "INFO"
            );

            return result;
        }

        public Task<int> DeleteAsync(int attendanceId)
        {
            if (attendanceId <= 0)
                throw new InvalidOperationException("Invalid AttendanceID.");

            return _repository.DeleteAsync(attendanceId);
        }
    }
}
