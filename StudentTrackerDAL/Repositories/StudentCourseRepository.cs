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

        // ============================================
        // CREATE
        // ============================================
        public async Task<int> CreateStudentCourseAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                IF EXISTS (SELECT 1 FROM Users WHERE UserID = @StudentID AND RoleID = 3)
                BEGIN
                    INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate, CreatedAt)
                    VALUES (@StudentID, @CourseID, GETDATE(), GETDATE());
                END";

            return await con.ExecuteAsync(sql, new { course.StudentID, course.CourseID });
        }

        // ============================================
        // GET ALL
        // ============================================
        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<StudentCourse>(
                "SELECT * FROM StudentCourses ORDER BY StudentCourseID DESC");
        }

        // ============================================
        // GET BY ID
        // ============================================
        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<StudentCourse>(
                "SELECT * FROM StudentCourses WHERE StudentCourseID = @Id",
                new { Id = id });
        }

        // ============================================
        // UPDATE
        // ============================================
        public async Task<int> UpdateAsync(StudentCourse course)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"UPDATE StudentCourses 
                        SET CourseID = @CourseID 
                        WHERE StudentCourseID = @StudentCourseID;";

            return await con.ExecuteAsync(sql, new
            {
                course.StudentCourseID,
                course.CourseID
            });
        }

        // ============================================
        // DELETE
        // ============================================
        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "DELETE FROM StudentCourses WHERE StudentCourseID = @Id",
                new { Id = id });
        }

        // ============================================
        // GET STUDENTS BY COURSE
        // ============================================
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

        // ============================================
        // COUNT BY COURSE
        // ============================================
        public async Task<int> CountByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT COUNT(*) 
                FROM StudentCourses sc 
                INNER JOIN Users u ON u.UserID = sc.StudentID 
                WHERE sc.CourseID = @CourseID AND u.RoleID = 3;";

            return await con.ExecuteScalarAsync<int>(sql, new { CourseID = courseId });
        }

        // ============================================
        // COUNT BY TEACHER
        // ============================================
        public async Task<int> CountByTeacherAsync(int teacherId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT COUNT(*) 
                FROM StudentCourses sc
                INNER JOIN Courses c ON c.CourseID = sc.CourseID
                INNER JOIN Users u ON u.UserID = sc.StudentID
                WHERE c.TeacherID = @TeacherID AND u.RoleID = 3;";

            return await con.ExecuteScalarAsync<int>(sql, new { TeacherID = teacherId });
        }
    }
}