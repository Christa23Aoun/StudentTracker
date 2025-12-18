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
        private readonly INotificationService _notificationService;

        private const int ROLE_TEACHER = 2;
        private const int ROLE_STUDENT = 3;

        public UserService(
            IUserRepository repo,
            INotificationService notificationService)
        {
            _repo = repo;
            _notificationService = notificationService;
        }

        public Task<IEnumerable<User>> GetAllAsync() => _repo.GetAllAsync();
        public Task<User?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task<User?> GetByEmailAsync(string email) => _repo.GetByEmailAsync(email);

        public async Task<int> CreateAsync(User user)
        {
            var userId = await _repo.CreateAsync(user);
            var createdUser = await _repo.GetByIdAsync(userId);

            if (createdUser == null)
                return userId;

            if (createdUser.RoleID == ROLE_STUDENT)
            {
                await _notificationService.NotifyStudentAsync(
                    userId,
                    "Your student account has been created.",
                    "ADMIN",
                    "/StudentDashboard/Profile"
                );
            }
            else if (createdUser.RoleID == ROLE_TEACHER)
            {
                await _notificationService.NotifyTeacherAsync(
                    userId,
                    "Your teacher account has been created.",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );
            }

            return userId;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var success = await _repo.UpdateAsync(user);
            if (!success)
                return false;

            var updatedUser = await _repo.GetByIdAsync(user.UserID);
            if (updatedUser == null)
                return true;

            if (updatedUser.RoleID == ROLE_STUDENT)
            {
                await _notificationService.NotifyStudentAsync(
                    updatedUser.UserID,
                    "Your profile was updated by the administration.",
                    "ADMIN",
                    "/StudentDashboard/Profile"
                );
            }
            else if (updatedUser.RoleID == ROLE_TEACHER)
            {
                await _notificationService.NotifyTeacherAsync(
                    updatedUser.UserID,
                    "Your profile was updated by the administration.",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return false;

            var success = await _repo.DeleteAsync(id);
            if (!success)
                return false;

            if (user.RoleID == ROLE_STUDENT)
            {
                await _notificationService.NotifyStudentAsync(
                    user.UserID,
                    "Your account has been deactivated.",
                    "ADMIN",
                    "/StudentDashboard/Profile"
                );
            }
            else if (user.RoleID == ROLE_TEACHER)
            {
                await _notificationService.NotifyTeacherAsync(
                    user.UserID,
                    "Your account has been deactivated.",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );
            }

            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return false;

            var success = await _repo.ActivateAsync(id);
            if (!success)
                return false;

            if (user.RoleID == ROLE_STUDENT)
            {
                await _notificationService.NotifyStudentAsync(
                    user.UserID,
                    "Your account has been reactivated.",
                    "ADMIN",
                    "/StudentDashboard/Profile"
                );
            }
            else if (user.RoleID == ROLE_TEACHER)
            {
                await _notificationService.NotifyTeacherAsync(
                    user.UserID,
                    "Your account has been reactivated.",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );
            }

            return true;
        }

        public Task<bool> SetRoleAsync(int id, int roleId) => _repo.SetRoleAsync(id, roleId);
        public Task<int> CountByRoleAsync(string roleName) => _repo.CountByRoleAsync(roleName);
    }
}
