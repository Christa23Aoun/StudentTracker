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

        public async Task<bool> ExistsAsync(int testId, int studentId)
        {
            using var con = new SqlConnection(_connectionString);
            var count = await con.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM TestGrades WHERE TestID = @testId AND StudentID = @studentId",
                new { testId, studentId }
            );
            return count > 0;
        }

        public async Task<TestGrade?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<TestGrade>(
                "SELECT * FROM TestGrades WHERE TestGradeID = @TestGradeID",
                new { TestGradeID = id });
        }

        public async Task<IEnumerable<TestGrade>> GetByTestWithStudentAsync(int testId, int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            string sql = @"
                SELECT 
                    tg.TestGradeID,
                    tg.TestID,
                    tg.StudentID,
                    u.FullName AS StudentName,
                    tg.Score,
                    tg.IsValidated,
                    t.CourseID
                FROM TestGrades tg
                JOIN Users u ON u.UserID = tg.StudentID
                JOIN Tests t ON t.TestID = tg.TestID
                WHERE tg.TestID = @TestID AND t.CourseID = @CourseID";

            return await con.QueryAsync<TestGrade>(sql, new { TestID = testId, CourseID = courseId });
        }

        public async Task<IEnumerable<TestGrade>> GetByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            string sql = @"
                SELECT 
                    tg.TestGradeID,
                    tg.TestID,
                    tg.StudentID,
                    u.FullName AS StudentName,
                    tg.Score,
                    tg.IsValidated,
                    t.CourseID
                FROM TestGrades tg
                JOIN Users u ON u.UserID = tg.StudentID
                JOIN Tests t ON t.TestID = tg.TestID
                WHERE t.CourseID = @CourseID";

            return await con.QueryAsync<TestGrade>(sql, new { CourseID = courseId });
        }

        public async Task<int> CreateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new
            {
                grade.TestID,
                grade.StudentID,
                grade.Score,
                grade.IsValidated
            };

            return await con.ExecuteScalarAsync<int>(
                "sp_CreateTestGrade",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);

            // Prevent updating validated grades
            var isValidated = await con.ExecuteScalarAsync<bool>(
                "SELECT IsValidated FROM TestGrades WHERE TestGradeID = @id",
                new { id = grade.TestGradeID }
            );

            if (isValidated)
                return -1;

            return await con.ExecuteAsync(@"
        UPDATE TestGrades
        SET Score = @Score,
            UpdatedAt = GETDATE()
        WHERE TestGradeID = @TestGradeID AND IsValidated = 0",
                grade);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.ExecuteAsync(
                "sp_DeleteTestGrade",
                new { TestGradeID = id },
                commandType: CommandType.StoredProcedure);
        }

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

        // =====================================================
        // FIXED: AVERAGE GRADE BY COURSE (NO VALIDATION REQUIRED)
        // =====================================================
        public async Task<decimal> GetAverageGradeByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.ExecuteScalarAsync<decimal?>(@"
                SELECT AVG(CAST(tg.Score AS FLOAT))
                FROM TestGrades tg
                JOIN Tests t ON tg.TestID = t.TestID
                WHERE t.CourseID = @CourseID",
                new { CourseID = courseId });

            return result ?? 0;
        }

        // =====================================================
        // NEW: AVERAGE GRADE BY SINGLE TEST
        // =====================================================
        public async Task<decimal> GetAverageByTestAsync(int testId)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.ExecuteScalarAsync<decimal?>(@"
                SELECT AVG(CAST(Score AS FLOAT))
                FROM TestGrades
                WHERE TestID = @TestID",
                new { TestID = testId });

            return result ?? 0;
        }
    }
}
