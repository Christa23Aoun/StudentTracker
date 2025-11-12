using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository _repository;

        public AttendanceService(string connectionString)
        {
            _repository = new AttendanceRepository(connectionString);
        }

        public Task<IEnumerable<Attendance>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Attendance?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<int> CreateAsync(Attendance att) => _repository.CreateAsync(att);
        public Task<int> UpdateAsync(Attendance att) => _repository.UpdateAsync(att);
        public Task<int> DeleteAsync(int id) => _repository.DeleteAsync(id);

    
        public Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId) =>
            _repository.GetByCourseIdAsync(courseId);
    }
}
