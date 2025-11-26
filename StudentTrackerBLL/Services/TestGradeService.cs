using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class TestGradeService
    {
        private readonly TestGradeRepository _repository;

        public TestGradeService(string connectionString)
        {
            _repository = new TestGradeRepository(connectionString);
        }
        public Task<IEnumerable<TestGrade>> GetGradesByTestAsync(int testId, int courseId)
        {
            return _repository.GetByTestAsync(testId, courseId);
        }
        public async Task<bool> ExistsAsync(int testId, int studentId)
        {
            return await _repository.ExistsAsync(testId, studentId);
        }

        public async Task<IEnumerable<TestGrade>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<TestGrade?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<IEnumerable<TestGrade>> GetByCourseAsync(int courseId) => await _repository.GetByCourseAsync(courseId);
        public async Task<bool> CreateAsync(TestGrade grade)
        {
            if (await _repository.ExistsAsync(grade.TestID, grade.StudentID))
                return false;

            await _repository.CreateAsync(grade);
            return true;
        }
        public async Task<int> UpdateAsync(TestGrade grade) => await _repository.UpdateAsync(grade);
        public async Task<int> DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
