using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class TestRepository : ITestRepository
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
            return await con.ExecuteScalarAsync<int>(
                "sp_CreateTest",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<TestDto>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<TestDto>("sp_GetAllTests", commandType: CommandType.StoredProcedure);
        }

        public async Task<TestDto?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<TestDto>(
                "sp_GetTestByID",
                new { TestID = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<TestDto>> GetByCourseIdAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<TestDto>(
                "sp_GetTestsByCourse",
                new { CourseID = courseId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Test?> GetModelByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<Test>(
                "sp_GetTestByID",
                new { TestID = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<Test>> GetModelsByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Test>(
                "sp_GetTestsByCourse",
                new { CourseID = courseId },
                commandType: CommandType.StoredProcedure
            );
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
            return await con.ExecuteAsync(
                "sp_DeleteTest",
                new { TestID = id },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<int> GetTeacherIdByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.ExecuteScalarAsync<int>(
                "SELECT TeacherID FROM Courses WHERE CourseID = @CourseID",
                new { CourseID = courseId });
        }
        public async Task<bool> ExistsAsync(int courseId, string testName, DateTime testDate)
        {
            using var con = new SqlConnection(_connectionString);

            var count = await con.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) 
          FROM Tests 
          WHERE CourseID = @CourseID 
            AND TestName = @TestName 
            AND TestDate = @TestDate",
                new
                {
                    CourseID = courseId,
                    TestName = testName,
                    TestDate = testDate
                });

            return count > 0;
        }

        public async Task<bool> HasValidatedGradesAsync(int testId)
        {
            using var con = new SqlConnection(_connectionString);

            var count = await con.ExecuteScalarAsync<int>(
                "sp_Test_HasValidatedGrades",
                new { TestID = testId },
                commandType: CommandType.StoredProcedure
            );

            return count > 0;
        }

    }
}
