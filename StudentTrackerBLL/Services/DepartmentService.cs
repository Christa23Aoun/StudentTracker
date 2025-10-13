using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Department>> GetAllAsync()
        => _repo.GetAllAsync();

    public Task<Department?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public Task<int> CreateAsync(DepartmentCreateDto dto)
        => _repo.CreateAsync(dto.DepartmentName);

    public Task<int> UpdateAsync(DepartmentUpdateDto dto)
        => _repo.UpdateAsync(dto.DepartmentID, dto.DepartmentName);

    public Task<int> DeleteAsync(int id)
        => _repo.DeleteAsync(id);
}
