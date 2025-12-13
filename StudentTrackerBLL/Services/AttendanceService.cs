using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository _repository;
        private readonly INotificationService _notificationService;

        public AttendanceService(
            string connectionString,
            INotificationService notificationService)
        {
            _repository = new AttendanceRepository(connectionString);
            _notificationService = notificationService;
        }

        public Task<IEnumerable<Attendance>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Attendance?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId) =>
            _repository.GetByCourseIdAsync(courseId);

        public async Task<int> CreateAsync(Attendance att)
        {
            var result = await _repository.CreateAsync(att);

            await _notificationService.NotifyStudentAsync(
                att.StudentID,
                "Attendance has been recorded for one of your courses.",
                "INFO"
            );

            return result;
        }

        public async Task<int> UpdateAsync(Attendance att)
        {
            var result = await _repository.UpdateAsync(att);

            await _notificationService.NotifyStudentAsync(
                att.StudentID,
                "Your attendance record has been updated.",
                "INFO"
            );

            return result;
        }

        public Task<int> DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
