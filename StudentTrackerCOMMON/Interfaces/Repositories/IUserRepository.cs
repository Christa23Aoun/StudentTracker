//using StudentTrackerCOMMON.Models;

//namespace StudentTrackerCOMMON.Interfaces.Repositories
//{
//    public interface IUserRepository
//    {
//        Task<int> CreateAsync(User user);
//        Task<User?> GetByEmailAsync(string email);
//        Task<User?> GetByIdAsync(int userId);

//        Task<bool> ActivateAsync(int userId);
//        Task<bool> SetRoleAsync(int userId, int roleId);
//        Task<bool> HardDeleteAsync(int userId);

//        Task<IEnumerable<User>> GetAllAsync();
//        Task<bool> UpdateAsync(User user);
//        Task<bool> DeleteAsync(int userId);

//        Task<int> CountByRoleAsync(string roleName);

//    }
//}
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<int> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int userId);

        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int userId);   // soft delete (IsActive = 0)

        Task<bool> ActivateAsync(int userId);
        Task<bool> SetRoleAsync(int userId, int roleId);

        Task<int> CountByRoleAsync(string roleName);
    }
}
