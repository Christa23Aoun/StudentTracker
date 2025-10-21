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

      public async Task<int> CreateAsync(string fullName, string email, string passwordHash, int roleId)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var id = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_Create",
                new
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = passwordHash,
                    RoleID = roleId
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
                    NewValue = email
                },
                commandType: CommandType.StoredProcedure);

            return id;
        }


        public async Task<User?> GetByEmailAsync(string email)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<User>(
                "dbo.sp_User_GetByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure);
        }

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
