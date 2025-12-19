using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseListItem>> GetAllAsync();
        Task<CourseListItem?> GetByIdAsync(int id);
        Task<IEnumerable<CourseListItem>> GetByTeacherIdAsync(int teacherId);
        Task<IEnumerable<dynamic>> GetCourseStatsByTeacherAsync(int teacherId);

        Task<int> CreateAsync(CourseCreateDto dto);
        Task<int> UpdateAsync(CourseUpdateDto dto);
        Task<int> DeleteAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
