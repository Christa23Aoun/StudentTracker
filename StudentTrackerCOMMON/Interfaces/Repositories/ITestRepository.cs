using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ITestRepository
    {
        Task<IEnumerable<TestDto>> GetAllAsync();
        Task<TestDto?> GetByIdAsync(int id);
        Task<IEnumerable<TestDto>> GetByCourseIdAsync(int courseId);

        Task<Test?> GetModelByIdAsync(int id);
        Task<IEnumerable<Test>> GetModelsByCourseAsync(int courseId);
        Task<int> GetTeacherIdByCourseAsync(int courseId);

        Task<int> CreateAsync(Test test);
        Task<int> UpdateAsync(Test test);
        Task<int> DeleteAsync(int id);
    }
}
