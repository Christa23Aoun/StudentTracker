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
            var email = dto.Email.Trim().ToLowerInvariant();

            var existing = await _users.GetByEmailAsync(email);
            if (existing is not null)
                throw new ArgumentException("Email already exists.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.");

            // Optionally: enforce minimum length/complexity here.

            // BCrypt with default work factor (usually 10–12 in libs). You can pass a work factor if you like.
            string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName?.Trim(),
                Email = email,
                PasswordHash = hash,
                RoleID = dto.RoleID,
                IsActive = true,                 // good default
                CreatedAt = DateTime.UtcNow,      // if your DB doesn’t set it
                UpdatedAt = null
            };

            return await _users.CreateAsync(user);
        }


        public async Task<bool> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _users.GetByEmailAsync(email);
            if (user is null || user.IsActive == false)
                return false;

            return BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        }

    }
}
