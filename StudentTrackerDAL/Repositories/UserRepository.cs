using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces;
using Microsoft.Extensions.Configuration;
using StudentTrackerCOMMON.Interfaces.Repositories;


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

        public async Task<int> CreateAsync(User user)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_Create",
                new
                {
                    user.FullName,
                    user.Email,
                    user.PasswordHash,
                    user.RoleID
                },
                commandType: CommandType.StoredProcedure);
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
            var rows = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_Activate",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure);
            return rows > 0;
        }

        public async Task<bool> SetRoleAsync(int userId, int roleId)
        {
            using var con = new SqlConnection(_connectionString);
            var rows = await con.ExecuteScalarAsync<int>(
                "dbo.sp_User_SetRole",
                new { UserID = userId, RoleID = roleId },
                commandType: CommandType.StoredProcedure);
            return rows > 0;
        }
    }
}
