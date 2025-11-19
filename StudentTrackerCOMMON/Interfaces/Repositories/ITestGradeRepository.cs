using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs.AdminDashboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ITestGradeRepository
    {
        Task<IEnumerable<TestGrade>> GetAllAsync();
        Task<TestGrade?> GetByIdAsync(int id);
        Task<int> CreateAsync(TestGrade grade);
        Task<int> UpdateAsync(TestGrade grade);
        Task<int> DeleteAsync(int id);

        Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync();
        Task<int> MarkGradeAsValidatedAsync(int testGradeId);
        Task<int> DeleteGradeAsync(int testGradeId);

        Task<decimal> GetAverageGradeByCourseAsync(int courseId);
    }
}
