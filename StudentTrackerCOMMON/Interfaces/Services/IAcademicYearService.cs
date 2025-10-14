using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Services;

public interface IAcademicYearService
{
    Task<IEnumerable<AcademicYear>> GetAllAsync();
    Task<AcademicYear?> GetByIdAsync(int id);
    Task<int> CreateAsync(AcademicYearCreateDto dto);
    Task<int> UpdateAsync(AcademicYearUpdateDto dto);
    Task<int> DeleteAsync(int id);
}
