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

            
            try
            {
                var parameters = new
                {
                    course.StudentID,
                    course.CourseID,
                    course.EnrollmentDate,
                    course.IsActive
                };

                return await con.ExecuteAsync(
                    "sp_CreateStudentCourse",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException)
            {
                var sql = @"
                    INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate, IsActive)
                    VALUES (@StudentID, @CourseID, @EnrollmentDate, @IsActive)";
                return await con.ExecuteAsync(sql, course);
            }
        }

        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);

            try
            {
                return await con.QueryAsync<StudentCourse>(
                    "sp_GetAllStudentCourses",
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException)
            {
                return await con.QueryAsync<StudentCourse>("SELECT * FROM StudentCourses");
            }
        }

        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);

            try
            {
                return await con.QueryFirstOrDefaultAsync<StudentCourse>(
                    "sp_GetStudentCourseByID",
                    new { StudentCourseID = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException)
            {
                return await con.QueryFirstOrDefaultAsync<StudentCourse>(
                    "SELECT * FROM StudentCourses WHERE StudentCourseID = @ID",
                    new { ID = id }
                );
            }
        }
        public async Task<IEnumerable<dynamic>> GetByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
        SELECT 
            sc.StudentCourseID,
            sc.StudentID,
            sc.CourseID,
            u.FullName AS StudentName
        FROM StudentCourses sc
        INNER JOIN Users u ON u.UserID = sc.StudentID
        WHERE sc.CourseID = @CourseID AND u.RoleID = 3
        ORDER BY u.FullName";

            return await con.QueryAsync(sql, new { CourseID = courseId });
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = "DELETE FROM StudentCourses WHERE StudentCourseID = @ID";
            return await con.ExecuteAsync(sql, new { ID = id });
        }


        public async Task<int> UpdateAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);

            try
            {
                var parameters = new
                {
                    course.StudentCourseID,
                    course.CourseID,
                    course.IsActive
                };

                return await con.ExecuteAsync(
                    "sp_UpdateStudentCourse",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException)
            {
                var sql = @"
                    UPDATE StudentCourses
                    SET CourseID = @CourseID,
                        IsActive = @IsActive
                    WHERE StudentCourseID = @StudentCourseID";
                return await con.ExecuteAsync(sql, course);
            }
        }

        public async Task<int> CountByTeacherAsync(int teacherId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
        SELECT COUNT(*) 
        FROM StudentCourses sc
        INNER JOIN Courses c ON c.CourseID = sc.CourseID
        INNER JOIN Users u ON u.UserID = sc.StudentID
        WHERE c.TeacherID = @TeacherID AND u.RoleID = 3";

            return await con.ExecuteScalarAsync<int>(sql, new { TeacherID = teacherId });
        }

    }

}
