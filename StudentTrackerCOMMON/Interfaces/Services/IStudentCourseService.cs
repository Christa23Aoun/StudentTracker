using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IStudentCourseService
    {
        Task<IEnumerable<StudentCourse>> GetAllAsync();
        Task<StudentCourse?> GetByIdAsync(int id);
        Task<int> CreateAsync(StudentCourse model);
        Task<int> UpdateAsync(StudentCourse model);
        Task<int> DeleteAsync(int id);

        Task<IEnumerable<StudentCourse>> GetCoursesByStudentAsync(int userId);
        Task<IEnumerable<StudentCourse>> GetByCourseAsync(int courseId);
        Task<bool> EnrollAsync(int userId, int courseId);
        Task<int> UnenrollAsync(int userId, int courseId);
    }
}
