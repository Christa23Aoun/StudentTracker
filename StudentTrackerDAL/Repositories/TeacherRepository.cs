using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public TeacherRepository(ISqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Teacher?> GetByEmailAsync(string email)
        {
            using var conn = _factory.Create();
            var sql = @"
                SELECT TOP 1 
                    UserID AS TeacherID,
                    FullName,
                    Email
                FROM Users
                WHERE Email = @Email AND RoleID = 2";
            return await conn.QueryFirstOrDefaultAsync<Teacher>(sql, new { Email = email });
        }
    }
}
