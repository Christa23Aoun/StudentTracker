using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseListItem>> GetAllAsync();
    Task<CourseListItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(CourseCreateDto dto);
    Task<int> UpdateAsync(CourseUpdateDto dto);
    Task<int> DeleteAsync(int id);
}
