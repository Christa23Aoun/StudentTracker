using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Services;

public interface ISemesterService
{
    Task<IEnumerable<Semester>> GetAllAsync();
    Task<Semester?> GetByIdAsync(int id);
    Task<int> CreateAsync(SemesterCreateDto dto);
    Task<int> UpdateAsync(SemesterUpdateDto dto);
    Task<int> DeleteAsync(int id);
}
