using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        // ✅ Read operations
        public Task<IEnumerable<User>> GetAllAsync() => _repo.GetAllAsync();
        public Task<User?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<User?> GetByEmailAsync(string email) => _repo.GetByEmailAsync(email);

        // ✅ Create returns new UserID (int)
        public Task<int> CreateAsync(User user) => _repo.CreateAsync(user);

        // ✅ Update / Delete / Activate / SetRole return bool (success/failure)
        public Task<bool> UpdateAsync(User user) => _repo.UpdateAsync(user);
        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);
        public Task<bool> ActivateAsync(int id) => _repo.ActivateAsync(id);
        public Task<bool> SetRoleAsync(int id, int roleId) => _repo.SetRoleAsync(id, roleId);

        // ✅ Optional: Count users by role
        public Task<int> CountByRoleAsync(string roleName) => _repo.CountByRoleAsync(roleName);
    }
}
