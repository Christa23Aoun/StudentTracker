using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public EnrollmentRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<StudentCourse>> GetCoursesByStudentAsync(int studentId)
        {
            using var con = _factory.Create();
            var sql = @"
                SELECT 
                    StudentCourseID,
                    StudentID,
                    CourseID,
                    EnrollmentDate,
                    IsActive
                FROM StudentCourses
                WHERE StudentID = @StudentID;";
            return await con.QueryAsync<StudentCourse>(sql, new { StudentID = studentId });
        }

        public async Task<IEnumerable<User>> GetStudentsByCourseAsync(int courseId)
        {
            using var con = _factory.Create();
            var sql = @"
                SELECT u.*
                FROM StudentCourses sc
                INNER JOIN Users u ON u.UserID = sc.StudentID
                WHERE sc.CourseID = @CourseID;";
            return await con.QueryAsync<User>(sql, new { CourseID = courseId });
        }

        public async Task<int> EnrollAsync(int studentId, int courseId)
        {
            using var con = _factory.Create();
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM StudentCourses WHERE StudentID=@StudentID AND CourseID=@CourseID)
                BEGIN
                    INSERT INTO StudentCourses(StudentID, CourseID, EnrollmentDate, IsActive)
                    VALUES(@StudentID, @CourseID, SYSDATETIME(), 1);
                    SELECT 1;
                END
                ELSE 
                BEGIN
                    SELECT 0;
                END";
            return await con.ExecuteScalarAsync<int>(sql, new { StudentID = studentId, CourseID = courseId });
        }

        public async Task<int> UnenrollAsync(int studentId, int courseId)
        {
            using var con = _factory.Create();
            var sql = "DELETE FROM StudentCourses WHERE StudentID=@StudentID AND CourseID=@CourseID;";
            return await con.ExecuteAsync(sql, new { StudentID = studentId, CourseID = courseId });
        }

        public async Task<int> BulkEnrollAsync(int studentId, IEnumerable<int> courseIds)
        {
            using var con = _factory.Create();
            using var tx = con.BeginTransaction();
            try
            {
                var count = 0;
                var sql = @"
                    IF NOT EXISTS (SELECT 1 FROM StudentCourses WHERE StudentID=@StudentID AND CourseID=@CourseID)
                    BEGIN
                        INSERT INTO StudentCourses(StudentID, CourseID, EnrollmentDate, IsActive)
                        VALUES(@StudentID, @CourseID, SYSDATETIME(), 1);
                        SELECT 1;
                    END
                    ELSE
                    BEGIN
                        SELECT 0;
                    END";

                foreach (var cid in courseIds)
                {
                    count += await con.ExecuteScalarAsync<int>(
                        sql,
                        new { StudentID = studentId, CourseID = cid },
                        tx
                    );
                }

                tx.Commit();
                return count;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
