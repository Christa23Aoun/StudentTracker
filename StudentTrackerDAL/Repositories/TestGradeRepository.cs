using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class TestGradeRepository : ITestGradeRepository
    {
        private readonly string _connectionString;

        public TestGradeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<TestGrade>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<TestGrade>("SELECT * FROM TestGrades");
        }

        public async Task<TestGrade?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<TestGrade>(
                "SELECT * FROM TestGrades WHERE TestGradeID = @TestGradeID",
                new { TestGradeID = id });
        }

        public async Task<int> CreateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(@"
                INSERT INTO TestGrades (TestID, StudentID, Score, IsValidated, IsRejected, CreatedAt, UpdatedAt)
                VALUES (@TestID, @StudentID, @Score, 0, 0, GETDATE(), GETDATE())",
                grade);
        }

        public async Task<int> UpdateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(@"
                UPDATE TestGrades
                SET Score = @Score,
                    UpdatedAt = GETDATE()
                WHERE TestGradeID = @TestGradeID",
                grade);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "DELETE FROM TestGrades WHERE TestGradeID = @TestGradeID",
                new { TestGradeID = id });
        }

        // Admin: Get pending grades
        public async Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync()
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<AdminPendingGradeItemDto>(@"
                SELECT tg.TestGradeID, tg.TestID, t.TestName,
                       t.CourseID, c.CourseName,
                       tg.StudentID, u.FullName AS StudentName,
                       tg.Score, tg.CreatedAt
                FROM TestGrades tg
                JOIN Tests t ON t.TestID = tg.TestID
                JOIN Courses c ON c.CourseID = t.CourseID
                JOIN Users u ON u.UserID = tg.StudentID
                WHERE tg.IsValidated = 0 AND tg.IsRejected = 0");
        }

        public async Task<int> MarkGradeAsValidatedAsync(int testGradeId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(@"
                UPDATE TestGrades
                SET IsValidated = 1,
                    ValidatedAt = GETDATE()
                WHERE TestGradeID = @TestGradeID",
                new { TestGradeID = testGradeId });
        }

        public async Task<int> DeleteGradeAsync(int testGradeId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(@"
                UPDATE TestGrades
                SET IsRejected = 1,
                    RejectedAt = GETDATE()
                WHERE TestGradeID = @TestGradeID",
                new { TestGradeID = testGradeId });
        }

        public async Task<decimal> GetAverageGradeByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.ExecuteScalarAsync<decimal?>(@"
                SELECT AVG(Score)
                FROM TestGrades tg
                JOIN Tests t ON tg.TestID = t.TestID
                WHERE t.CourseID = @CourseID AND tg.IsValidated = 1",
                new { CourseID = courseId });

            return result ?? 0;
        }
    }
}
