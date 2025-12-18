using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public TeacherRepository(ISqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<Teacher>(
                "SELECT * FROM Teachers");
        }

        public async Task<Teacher?> GetByIdAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<Teacher>(
                "SELECT * FROM Teachers WHERE TeacherID = @TeacherID",
                new { TeacherID = id });
        }

        public async Task<Teacher?> GetByEmailAsync(string email)
        {
            using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<Teacher>(
                "SELECT * FROM Teachers WHERE Email = @Email",
                new { Email = email });
        }

        public async Task<int?> GetTeacherIdByUserIdAsync(int userId)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int?>(
                "SELECT TeacherID FROM Teachers WHERE UserID = @UserID",
                new { UserID = userId });
        }
    }
}
