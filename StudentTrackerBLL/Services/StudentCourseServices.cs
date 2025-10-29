using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseService
    {
        private readonly string _connectionString;

        public StudentCourseService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // ✅ Get all student-course records
        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<StudentCourse>("SELECT * FROM StudentCourses");
        }

        // ✅ Get by ID
        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM StudentCourses WHERE StudentCourseID = @ID";
            return await con.QueryFirstOrDefaultAsync<StudentCourse>(sql, new { ID = id });
        }

        // ✅ Create
        public async Task<int> CreateAsync(StudentCourse entity)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate)
                        VALUES (@StudentID, @CourseID, @EnrollmentDate)";
            return await con.ExecuteAsync(sql, entity);
        }

        // ✅ Update
        public async Task<int> UpdateAsync(StudentCourse entity)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"UPDATE StudentCourses 
                        SET StudentID = @StudentID, CourseID = @CourseID, EnrollmentDate = @EnrollmentDate
                        WHERE StudentCourseID = @StudentCourseID";
            return await con.ExecuteAsync(sql, entity);
        }

        // ✅ Delete
        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync("DELETE FROM StudentCourses WHERE StudentCourseID = @ID", new { ID = id });
        }

        // ✅ Get all courses for a specific student (for Student Dashboard)
        public async Task<IEnumerable<dynamic>> GetCoursesByStudentAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    sc.StudentCourseID,
                    c.CourseID,
                    c.CourseName,
                    d.DepartmentName AS Department,
                    u.FullName AS TeacherName,
                    s.Name AS Semester
                FROM StudentCourses sc
                INNER JOIN Courses c ON c.CourseID = sc.CourseID
                INNER JOIN Departments d ON d.DepartmentID = c.DepartmentID
                INNER JOIN Users u ON u.UserID = c.TeacherID
                INNER JOIN Semesters s ON s.SemesterID = c.SemesterID
                WHERE sc.StudentID = @StudentID";

            var result = await con.QueryAsync(sql, new { StudentID = studentId });
            return result;
        }

        // ✅ Enroll a student in a course (for Student Dashboard)
        public async Task<bool> EnrollAsync(int studentId, int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM StudentCourses WHERE StudentID = @StudentID AND CourseID = @CourseID)
                BEGIN
                    INSERT INTO StudentCourses (StudentID, CourseID, EnrollmentDate)
                    VALUES (@StudentID, @CourseID, GETDATE())
                END";
            var rows = await con.ExecuteAsync(sql, new { StudentID = studentId, CourseID = courseId });
            return rows > 0;
        }
    }
}
