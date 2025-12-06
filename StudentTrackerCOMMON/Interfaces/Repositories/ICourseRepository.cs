using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ICourseRepository
    {
        Task<IEnumerable<CourseListItem>> GetAllAsync();
        Task<CourseListItem?> GetByIdAsync(int id);
        Task<int> CreateAsync(Course entity);
        Task<int> UpdateAsync(Course entity);
        Task<int> DeleteAsync(int id);
        Task<int> DeactivateAsync(int id);

        Task<List<Course>> GetByTeacherIdAsync(int teacherId);
        Task<List<User>> GetEnrolledStudentsAsync(int courseId);

        Task<int> CountActiveAsync();
        Task<IEnumerable<dynamic>> GetCourseSummaryAsync();
        Task<IEnumerable<dynamic>> GetCourseStatsByTeacherAsync(int teacherId);

        Task<IEnumerable<CourseSchedule>> GetSchedulesAsync(int courseId);
    }
}
