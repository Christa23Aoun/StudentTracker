using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly IAcademicYearRepository _repo;
    public AcademicYearService(IAcademicYearRepository repo) => _repo = repo;

    public Task<IEnumerable<AcademicYear>> GetAllAsync() => _repo.GetAllAsync();
    public Task<AcademicYear?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task<int> CreateAsync(AcademicYearCreateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("EndDate must be after StartDate.");
        return _repo.CreateAsync(dto.Name, dto.StartDate, dto.EndDate);
    }

    public Task<int> UpdateAsync(AcademicYearUpdateDto dto)
    {
        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("EndDate must be after StartDate.");
        return _repo.UpdateAsync(dto.AcademicYearID, dto.Name, dto.StartDate, dto.EndDate);
    }

    public Task<int> DeleteAsync(int id) => _repo.DeleteAsync(id);
}
