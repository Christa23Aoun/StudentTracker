using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace StudentTrackerDAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                 ?? throw new ArgumentNullException(nameof(config), "Missing database connection string");
        }

        // ✅ Create new user
        public async Task<int> CreateAsync(User user)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var id = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_Create",
                new
                {
                    user.FullName,
                    user.Email,
                    user.PasswordHash,
                    user.RoleID
                },
                commandType: CommandType.StoredProcedure);

            await con.ExecuteAsync(
                "dbo.sp_AuditLog_Add",
                new
                {
                    UserID = id,
                    Action = "INSERT",
                    TableName = "Users",
                    RecordID = id.ToString(),
                    OldValue = (string?)null,
                    NewValue = user.Email
                },
                commandType: CommandType.StoredProcedure);

            return id;
        }

        // ✅ Retrieve all users (for MVC list)
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<User>(
                "SELECT * FROM Users ORDER BY UserID DESC");
        }

        // ✅ Retrieve user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<User>(
                "dbo.sp_User_GetByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
        }

        // ✅ Activate user account
        public async Task<bool> ActivateAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var rows = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_Activate",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure);

            if (rows > 0)
            {
                await con.ExecuteAsync(
                    "dbo.sp_AuditLog_Add",
                    new
                    {
                        UserID = userId,
                        Action = "UPDATE",
                        TableName = "Users",
                        RecordID = userId.ToString(),
                        OldValue = "IsActive=0",
                        NewValue = "IsActive=1"
                    },
                    commandType: CommandType.StoredProcedure);
            }

            return rows > 0;
        }

        // ✅ Change role
        public async Task<bool> SetRoleAsync(int userId, int roleId)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var rows = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_SetRole",
                new { UserID = userId, RoleID = roleId },
                commandType: CommandType.StoredProcedure);

            if (rows > 0)
            {
                await con.ExecuteAsync(
                    "dbo.sp_AuditLog_Add",
                    new
                    {
                        UserID = userId,
                        Action = "UPDATE",
                        TableName = "Users",
                        RecordID = userId.ToString(),
                        OldValue = "Role changed",
                        NewValue = $"RoleID={roleId}"
                    },
                    commandType: CommandType.StoredProcedure);
            }

            return rows > 0;
        }

        // ✅ Update existing user
        public async Task<bool> UpdateAsync(User user)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var rows = await con.ExecuteAsync(
                "UPDATE Users SET FullName=@FullName, Email=@Email, RoleID=@RoleID, IsActive=@IsActive WHERE UserID=@UserID",
                new
                {
                    user.FullName,
                    user.Email,
                    user.RoleID,
                    user.IsActive,
                    user.UserID
                });

            if (rows > 0)
            {
                await con.ExecuteAsync(
                    "dbo.sp_AuditLog_Add",
                    new
                    {
                        UserID = user.UserID,
                        Action = "UPDATE",
                        TableName = "Users",
                        RecordID = user.UserID.ToString(),
                        OldValue = "User data modified",
                        NewValue = $"{user.FullName}, {user.Email}"
                    },
                    commandType: CommandType.StoredProcedure);
            }

            return rows > 0;
        }

        // ✅ Soft Delete User (recommended)
        public async Task<bool> DeleteAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // Set user inactive instead of deleting to avoid FK conflicts
            var rows = await con.ExecuteAsync(
                "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID",
                new { UserID = userId });

            if (rows > 0)
            {
                await con.ExecuteAsync(
                    "dbo.sp_AuditLog_Add",
                    new
                    {
                        UserID = userId,
                        Action = "UPDATE",
                        TableName = "Users",
                        RecordID = userId.ToString(),
                        OldValue = "IsActive=1",
                        NewValue = "IsActive=0"
                    },
                    commandType: CommandType.StoredProcedure);
            }

            return rows > 0;
        }

        public async Task<int> CountByRoleAsync(string roleName)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var sql = @"
        SELECT COUNT(*)
        FROM Users u
        INNER JOIN Roles r ON u.RoleID = r.RoleID
        WHERE r.RoleName = @roleName AND u.IsActive = 1;";

            return await con.ExecuteScalarAsync<int>(sql, new { roleName });
        }



    }
}
   