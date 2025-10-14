using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories;

public interface ISemesterRepository
{
    Task<IEnumerable<Semester>> GetAllAsync();
    Task<Semester?> GetByIdAsync(int id);
    Task<int> CreateAsync(int academicYearID, string name, int semesterNb, DateTime startDate, DateTime endDate);
    Task<int> UpdateAsync(int semesterID, int academicYearID, string name, int semesterNb, DateTime startDate, DateTime endDate);
    Task<int> DeleteAsync(int id);
}
