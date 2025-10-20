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

        // 🟢 Create user + AuditLog
        public async Task<int> CreateAsync(User user)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            // 1️⃣ Create the user through stored procedure
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

            // 2️⃣ Log the creation in AuditLog
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

        // 🟢 Get user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<User>(
                "dbo.sp_User_GetByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
        }

        // 🟢 Activate user + AuditLog
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

        // 🟢 Set role + AuditLog
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
    }
}
