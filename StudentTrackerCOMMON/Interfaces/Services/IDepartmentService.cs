using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<int> CreateAsync(DepartmentCreateDto dto);
    Task<int> UpdateAsync(DepartmentUpdateDto dto);
    Task<int> DeleteAsync(int id);
}
