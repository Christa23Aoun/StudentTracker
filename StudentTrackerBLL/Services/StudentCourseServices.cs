using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseService
    {
        private readonly StudentCourseRepository _repo;
        private readonly string _connectionString;

        public StudentCourseService(string connectionString)
        {
            _connectionString = connectionString;
            _repo = new StudentCourseRepository(connectionString);
        }

        public async Task<IEnumerable<StudentCourse>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<StudentCourse?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task<int> CreateAsync(StudentCourse entity) => await _repo.CreateStudentCourseAsync(entity);

        public async Task<int> UpdateAsync(StudentCourse entity) => await _repo.UpdateAsync(entity);

        public async Task<int> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<IEnumerable<dynamic>> GetByCourseAsync(int courseId)
        {
            return await _repo.GetByCourseAsync(courseId);
        }

        public async Task<int> CountByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = "SELECT COUNT(*) FROM StudentCourses WHERE CourseID = @CourseID;";
            return await con.ExecuteScalarAsync<int>(sql, new { CourseID = courseId });
        }

        public async Task<int> CountByTeacherAsync(int teacherId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT COUNT(*) 
                FROM StudentCourses sc
                WHERE sc.CourseID IN (
                    SELECT CourseID FROM Courses WHERE TeacherID = @TeacherID
                );";
            return await con.ExecuteScalarAsync<int>(sql, new { TeacherID = teacherId });
        }

        public async Task<bool> EnrollAsync(int studentId, int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM StudentCourses WHERE StudentID = @StudentID AND CourseID = @CourseID)
                BEGIN
                    INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate)
                    VALUES (@StudentID, @CourseID, GETDATE());
                END";

            var rows = await con.ExecuteAsync(sql, new { StudentID = studentId, CourseID = courseId });
            return rows > 0;
        }
    }
}