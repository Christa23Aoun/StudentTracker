using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;
    public SemesterService(ISemesterRepository repo) => _repo = repo;

    public Task<IEnumerable<Semester>> GetAllAsync() => _repo.GetAllAsync();
    public Task<Semester?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task<int> CreateAsync(SemesterCreateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("EndDate must be after StartDate.");
        if (dto.SemesterNb <= 0)
            throw new ArgumentException("SemesterNb must be positive.");
        return _repo.CreateAsync(dto.AcademicYearID, dto.Name, dto.SemesterNb, dto.StartDate, dto.EndDate);
    }

    public Task<int> UpdateAsync(SemesterUpdateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("EndDate must be after StartDate.");
        if (dto.SemesterNb <= 0)
            throw new ArgumentException("SemesterNb must be positive.");
        return _repo.UpdateAsync(dto.SemesterID, dto.AcademicYearID, dto.Name, dto.SemesterNb, dto.StartDate, dto.EndDate);
    }

    public Task<int> DeleteAsync(int id) => _repo.DeleteAsync(id);
}
