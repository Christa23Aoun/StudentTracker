using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);

        Task<int> CreateAsync(User user);

        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);

        Task<bool> ActivateAsync(int id);
        Task<bool> SetRoleAsync(int id, int roleId);

        Task<int> CountByRoleAsync(string roleName);
    }
}
