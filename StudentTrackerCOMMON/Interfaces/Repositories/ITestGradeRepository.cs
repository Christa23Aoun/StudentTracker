using StudentTrackerCOMMON.Models;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ITestGradeRepository
    {
        Task<decimal> GetAverageGradeByCourseAsync(int courseId);
        Task<IEnumerable<TestGrade>> GetAllAsync();

    }
}
