using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories;

public interface IAcademicYearRepository
{
    Task<IEnumerable<AcademicYear>> GetAllAsync();
    Task<AcademicYear?> GetByIdAsync(int id);
    Task<int> CreateAsync(string name, DateTime startDate, DateTime endDate);
    Task<int> UpdateAsync(int id, string name, DateTime startDate, DateTime endDate);
    Task<int> DeleteAsync(int id);
}
