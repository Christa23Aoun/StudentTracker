using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using System.Data;

namespace StudentTrackerDAL.Repositories
{
    public class StudentCourseRepository
    {
        private readonly string _connectionString;

        public StudentCourseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateStudentCourseAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                course.StudentID,
                course.CourseID,
                course.EnrollmentDate,
                course.IsActive
            };
            return await con.ExecuteAsync("sp_CreateStudentCourse", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<StudentCourse>("sp_GetAllStudentCourses", commandType: CommandType.StoredProcedure);
        }

        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<StudentCourse>("sp_GetStudentCourseByID", new { StudentCourseID = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                course.StudentCourseID,
                course.CourseID,
                course.IsActive
            };
            return await con.ExecuteAsync("sp_UpdateStudentCourse", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync("sp_DeleteStudentCourse", new { StudentCourseID = id }, commandType: CommandType.StoredProcedure);
        }
    }
}
