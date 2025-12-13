using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class TestGradeService
    {
        private readonly ITestGradeRepository _repository;
        private readonly ITestRepository _testRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly INotificationService _notificationService;

        public TestGradeService(
            ITestGradeRepository repository,
            ITestRepository testRepository,
            ICourseRepository courseRepository,
            INotificationService notificationService)
        {
            _repository = repository;
            _testRepository = testRepository;
            _courseRepository = courseRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<TestGrade>> GetGradesByTestAsync(int testId, int courseId)
            => await _repository.GetByTestWithStudentAsync(testId, courseId);

        public async Task<bool> ExistsAsync(int testId, int studentId)
            => await _repository.ExistsAsync(testId, studentId);

        public async Task<IEnumerable<TestGrade>> GetAllAsync()
            => await _repository.GetAllAsync();

        public async Task<TestGrade?> GetByIdAsync(int id)
            => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<TestGrade>> GetByCourseAsync(int courseId)
            => await _repository.GetByCourseAsync(courseId);

        public async Task<bool> CreateAsync(TestGrade grade)
        {
            if (await _repository.ExistsAsync(grade.TestID, grade.StudentID))
                return false;

            await _repository.CreateAsync(grade);

            var test = await _testRepository.GetModelByIdAsync(grade.TestID);
            var course = await _courseRepository.GetByIdAsync(test.CourseID);

            await _notificationService.NotifyStudentAsync(
                grade.StudentID,
                $"A new grade has been added for the test '{test.TestName}' in the course '{course.CourseName}'.",
                "GRADE"
            );

            return true;
        }

        public async Task<int> UpdateAsync(TestGrade grade)
        {
            var existing = await _repository.GetByIdAsync(grade.TestGradeID);
            if (existing == null)
                return -1;

            if (existing.IsValidated)
                return -2;

            return await _repository.UpdateAsync(grade);
        }

        public async Task<int> DeleteAsync(int id)
            => await _repository.DeleteAsync(id);

        public async Task<decimal> GetAverageByCourseAsync(int courseId)
            => await _repository.GetAverageGradeByCourseAsync(courseId);

        public async Task<decimal> GetAverageByTestAsync(int testId)
            => await _repository.GetAverageByTestAsync(testId);
    }
}
