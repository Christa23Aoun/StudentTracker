using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class TestGradeService
    {
        private readonly TestGradeRepository _repository;

        public TestGradeService(string connectionString)
        {
            _repository = new TestGradeRepository(connectionString);
        }

        public async Task<IEnumerable<TestGrade>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<TestGrade?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<int> CreateAsync(TestGrade grade) => await _repository.CreateAsync(grade);
        public async Task<int> UpdateAsync(TestGrade grade) => await _repository.UpdateAsync(grade);
        public async Task<int> DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
