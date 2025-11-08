using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ActivateAsync(int userId);
        Task<bool> SetRoleAsync(int userId, int roleId);

        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);

        Task<int> CountByRoleAsync(string roleName);

    }
}
