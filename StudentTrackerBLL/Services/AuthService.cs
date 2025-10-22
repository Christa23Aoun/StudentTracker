using BCrypt.Net;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        public AuthService(IUserRepository users) => _users = users;

        public async Task<int> RegisterAsync(RegisterDto dto)
        {
            
            var existing = await _users.GetByEmailAsync(dto.Email);
            if (existing is not null)
                throw new ArgumentException("Email already exists.");

            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hash,
                RoleID = dto.RoleID
            };

            return await _users.CreateAsync(user);
        }

        public async Task<bool> LoginAsync(LoginDto dto)
        {
            var user = await _users.GetByEmailAsync(dto.Email);
            if (user is null || user.IsActive == false)
                return false;

            return BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        }
    }
}
