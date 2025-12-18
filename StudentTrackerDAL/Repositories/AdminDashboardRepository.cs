using Dapper;
using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerDAL.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public AdminDashboardRepository(ISqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync()
        {
            using var con = _factory.Create();
            var sql = @"
        SELECT 
            tg.TestGradeID,
            tg.TestID,
            t.TestName,
            c.CourseID,
            c.CourseName,
            u.UserID AS StudentID,
            u.FullName AS StudentName,
            tg.Score,
            tg.CreatedAt
        FROM TestGrades tg
        INNER JOIN Tests t ON tg.TestID = t.TestID
        INNER JOIN Courses c ON t.CourseID = c.CourseID
        INNER JOIN Users u ON tg.StudentID = u.UserID
        WHERE ISNULL(tg.IsValidated, 0) = 0;";

            return await con.QueryAsync<AdminPendingGradeItemDto>(sql);
        }


        public async Task<bool> ValidateGradeAsync(int testGradeId)
        {
            using var con = _factory.Create();
            var sql = @"
                UPDATE TestGrades 
                SET IsValidated = 1, UpdatedAt = SYSDATETIME()
                WHERE TestGradeID = @id";

            return await con.ExecuteAsync(sql, new { id = testGradeId }) > 0;
        }

        public async Task<bool> RejectGradeAsync(int testGradeId)
        {
            using var con = _factory.Create();
            var sql = "DELETE FROM TestGrades WHERE TestGradeID = @id";

            return await con.ExecuteAsync(sql, new { id = testGradeId }) > 0;
        }

        public async Task ValidateAllPendingAsync()
        {
            using var con = _factory.Create();

            await con.ExecuteAsync(
                "dbo.TestGrades_ValidateAllPending",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CountPendingGradesAsync()
        {
            using var con = _factory.Create();

            var sql = @"
                SELECT COUNT(*) 
                FROM TestGrades 
                WHERE ISNULL(IsValidated, 0) = 0";

            return await con.ExecuteScalarAsync<int>(sql);
        }
    }
}
