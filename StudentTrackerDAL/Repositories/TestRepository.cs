using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs;
using System.Data;

namespace StudentTrackerDAL.Repositories
{
    public class TestRepository
    {
        private readonly string _connectionString;

        public TestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(Test test)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                test.CourseID,
                test.TestName,
                test.TestDate,
                test.Weight,
                test.MaxScore
            };
            return await con.ExecuteAsync("sp_CreateTest", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<TestDto>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<TestDto>("sp_GetAllTests", commandType: CommandType.StoredProcedure);
        }

        public async Task<TestDto?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<TestDto>("sp_GetTestByID", new { TestID = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<TestDto>> GetByCourseIdAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            string sql = @"
                SELECT 
                    t.TestID,
                    t.CourseID,
                    c.CourseName,
                    t.TestName,
                    t.TestDate,
                    t.Weight,
                    t.MaxScore
                FROM Tests t
                INNER JOIN Courses c ON t.CourseID = c.CourseID
                WHERE t.CourseID = @CourseID
                ORDER BY t.TestDate DESC";

            return await con.QueryAsync<TestDto>(sql, new { CourseID = courseId });
        }

        public async Task<int> UpdateAsync(Test test)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                test.TestID,
                test.TestName,
                test.TestDate,
                test.Weight,
                test.MaxScore
            };
            return await con.ExecuteAsync("sp_UpdateTest", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync("sp_DeleteTest", new { TestID = id }, commandType: CommandType.StoredProcedure);
        }
    }
}
