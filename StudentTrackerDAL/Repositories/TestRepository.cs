using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
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

        public async Task<IEnumerable<Test>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Test>("sp_GetAllTests", commandType: CommandType.StoredProcedure);
        }

        public async Task<Test?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<Test>("sp_GetTestByID", new { TestID = id }, commandType: CommandType.StoredProcedure);
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
