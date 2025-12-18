using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public CourseRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<CourseListItem>> GetAllAsync()
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<CourseListItem>(
                "dbo.Courses_GetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CourseListItem?> GetByIdAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<CourseListItem>(
                "dbo.Courses_GetById",
                new { CourseID = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(Course entity)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Courses_Create",
                new
                {
                    entity.CourseCode,
                    entity.CourseName,
                    entity.CreditHours,
                    entity.DepartmentID,
                    entity.SemesterID,
                    entity.TeacherID,
                    entity.IsActive
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(Course entity)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteAsync(
                "dbo.Courses_Update",
                new
                {
                    entity.CourseID,
                    entity.CourseCode,
                    entity.CourseName,
                    entity.CreditHours,
                    entity.DepartmentID,
                    entity.SemesterID,
                    entity.TeacherID,
                    entity.IsActive
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteAsync(
                "dbo.Courses_Delete",
                new { CourseID = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeactivateAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteAsync(
                "dbo.Courses_Deactivate",
                new { CourseID = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<Course>> GetByTeacherIdAsync(int teacherId)
        {
            using var conn = _factory.Create();
            var sql = @"
                SELECT 
                    c.CourseID,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    c.DepartmentID,
                    c.SemesterID,
                    c.TeacherID,
                    c.IsActive
                FROM Courses c
                WHERE c.TeacherID = @TeacherID
                  AND c.IsActive = 1";

            var result = await conn.QueryAsync<Course>(sql, new { TeacherID = teacherId });
            return result.ToList();
        }

        public async Task<List<User>> GetEnrolledStudentsAsync(int courseId)
        {
            using var conn = _factory.Create();
            var result = await conn.QueryAsync<User>(
                @"SELECT u.*
                  FROM StudentCourses sc
                  INNER JOIN Users u ON sc.StudentID = u.UserID
                  WHERE sc.CourseID = @CourseID",
                new { CourseID = courseId });
            return result.ToList();
        }

        public async Task<int> CountActiveAsync()
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Courses WHERE IsActive = 1");
        }

        public async Task<IEnumerable<dynamic>> GetCourseSummaryAsync()
        {
            using var conn = _factory.Create();
            var sql = @"
                SELECT 
                    c.CourseID,
                    c.CourseName,
                    d.DepartmentName,
                    u.FullName AS TeacherName,
                    c.IsActive
                FROM Courses c
                INNER JOIN Departments d ON d.DepartmentID = c.DepartmentID
                INNER JOIN Users u ON u.UserID = c.TeacherID
                ORDER BY c.CourseName;";
            return await conn.QueryAsync(sql);
        }

        public async Task<IEnumerable<TeacherCourseRowDto>> GetCourseStatsByTeacherAsync(int teacherId)
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<TeacherCourseRowDto>(
                "dbo.sp_TeacherDashboard_GetCourseStats",
                new { TeacherId = teacherId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<CourseSchedule>> GetSchedulesAsync(int courseId)
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<CourseSchedule>(
                "dbo.CourseSchedule_GetByCourse",
                new { CourseID = courseId },
                commandType: CommandType.StoredProcedure);
        }
    }
}
