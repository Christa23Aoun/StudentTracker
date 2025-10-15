using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using System.Data;

namespace StudentTrackerDAL.Repositories
{
    public class TestGradeRepository
    {
        private readonly string _connectionString;

        public TestGradeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                grade.TestID,
                grade.StudentID,
                grade.Score
            };
            return await con.ExecuteAsync("sp_CreateTestGrade", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<TestGrade>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<TestGrade>("sp_GetAllTestGrades", commandType: CommandType.StoredProcedure);
        }

        public async Task<TestGrade?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<TestGrade>("sp_GetTestGradeByID", new { TestGradeID = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(TestGrade grade)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                grade.TestGradeID,
                grade.Score,
                grade.IsValidated
            };
            return await con.ExecuteAsync("sp_UpdateTestGrade", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync("sp_DeleteTestGrade", new { TestGradeID = id }, commandType: CommandType.StoredProcedure);
        }
    }
}
