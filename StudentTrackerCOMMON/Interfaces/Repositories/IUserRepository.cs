using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ActivateAsync(int userId);
        Task<bool> SetRoleAsync(int userId, int roleId);
    }
}
