using BCrypt.Net;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerBLL.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    public AuthService(IUserRepository users) => _users = users;

    public async Task<int> RegisterAsync(RegisterDto dto)
    {
        // 1) prevent duplicates
        var existing = await _users.GetByEmailAsync(dto.Email);
        if (existing is not null) throw new ArgumentException("Email already exists.");

        // 2) hash password
        string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // 3) create
        return await _users.CreateAsync(dto.FullName, dto.Email, hash, dto.RoleID);
    }

    public async Task<bool> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email);
        if (user is null || user.IsActive == false) return false;

        return BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
    }
}
