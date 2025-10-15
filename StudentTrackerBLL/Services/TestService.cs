using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class TestService
    {
        private readonly TestRepository _repository;

        public TestService(string connectionString)
        {
            _repository = new TestRepository(connectionString);
        }

        public async Task<IEnumerable<Test>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<Test?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<int> CreateAsync(Test t)
        {
            if (t.Weight <= 0 || t.MaxScore <= 0)
                throw new ArgumentException("Invalid test values.");
            return await _repository.CreateAsync(t);
        }

        public async Task<int> UpdateAsync(Test t) => await _repository.UpdateAsync(t);
        public async Task<int> DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
