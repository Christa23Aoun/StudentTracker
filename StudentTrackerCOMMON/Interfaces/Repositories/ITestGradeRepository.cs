using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ITestGradeRepository
    {
        Task<IEnumerable<TestGrade>> GetAllAsync();
        Task<TestGrade?> GetByIdAsync(int id);
        Task<IEnumerable<TestGrade>> GetByCourseAsync(int courseId);
        Task<IEnumerable<TestGrade>> GetByTestWithStudentAsync(int testId, int courseId);
        Task<bool> ExistsAsync(int testId, int studentId);

        Task<int> CreateAsync(TestGrade grade);
        Task<int> UpdateAsync(TestGrade grade);
        Task<int> DeleteAsync(int id);

        Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync();
        Task<int> MarkGradeAsValidatedAsync(int testGradeId);
        Task<int> DeleteGradeAsync(int testGradeId);

        Task<decimal> GetAverageGradeByCourseAsync(int courseId);
        Task<decimal> GetAverageByTestAsync(int testId);

        Task<CourseStatsDto> GetCourseStatsAsync(int courseId);
    }
}
