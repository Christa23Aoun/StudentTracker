using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<int> CreateAsync(string fullName, string email, string passwordHash, int roleId);
        Task<bool> ActivateAsync(int userId);
        Task<bool> SetRoleAsync(int userId, int roleId);
    }
}
