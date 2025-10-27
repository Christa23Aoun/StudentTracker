using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ActivateAsync(int userId);
        Task<bool> SetRoleAsync(int userId, int roleId);

        // If you added these for the MVC list/edit/delete views:
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);

        Task<int> CountByRoleAsync(string roleName);

    }
}
