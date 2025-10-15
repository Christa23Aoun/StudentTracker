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

        public async Task<IEnumerable<Attendance>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<Attendance?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<int> CreateAsync(Attendance att) => await _repository.CreateAsync(att);
        public async Task<int> UpdateAsync(Attendance att) => await _repository.UpdateAsync(att);
        public async Task<int> DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
