using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<int> CreateAsync(string departmentName);
    Task<int> UpdateAsync(int id, string departmentName);
    Task<int> DeleteAsync(int id);
}
