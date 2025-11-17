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

            var sql = @"
                IF EXISTS (SELECT 1 FROM Users WHERE UserID = @StudentID AND RoleID = 3)
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM StudentCourses 
                        WHERE StudentID = @StudentID AND CourseID = @CourseID
                    )
                    BEGIN
                        INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate, CreatedAt, IsActive)
                        VALUES (@StudentID, @CourseID, GETDATE(), GETDATE(), 1);
                    END
                END";

            return await con.ExecuteAsync(sql, new { course.StudentID, course.CourseID });
        }

        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<StudentCourse>(
                "SELECT * FROM StudentCourses ORDER BY StudentCourseID DESC");
        }

        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryFirstOrDefaultAsync<StudentCourse>(
                "SELECT * FROM StudentCourses WHERE StudentCourseID = @ID",
                new { ID = id });
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
                ORDER BY u.FullName;";

            return await con.QueryAsync(sql, new { CourseID = courseId });
        }

        public async Task<int> UpdateAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE StudentCourses 
                SET CourseID = @CourseID
                WHERE StudentCourseID = @StudentCourseID";

            return await con.ExecuteAsync(sql, new
            {
                course.StudentCourseID,
                course.CourseID
            });
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = "DELETE FROM StudentCourses WHERE StudentCourseID = @Id";
            return await con.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<int> CountByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*) 
                FROM StudentCourses sc
                INNER JOIN Users u ON u.UserID = sc.StudentID
                WHERE sc.CourseID = @CourseID AND u.RoleID = 3";

            return await con.ExecuteScalarAsync<int>(sql, new { CourseID = courseId });
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
