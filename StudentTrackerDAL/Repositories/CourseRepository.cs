using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ISqlConnectionFactory _factory;
    public CourseRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<CourseListItem>> GetAllAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<CourseListItem>(
            "dbo.Courses_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<CourseListItem?> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.QueryFirstOrDefaultAsync<CourseListItem>(
            "dbo.Courses_GetById",
            new { CourseID = id },
            commandType: System.Data.CommandType.StoredProcedure);
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
                entity.TeacherID,
                entity.SemesterID,
                entity.IsActive
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(Course entity)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Courses_Update",
            new
            {
                entity.CourseID,
                entity.CourseCode,
                entity.CourseName,
                entity.CreditHours,
                entity.DepartmentID,
                entity.TeacherID,
                entity.SemesterID,
                entity.IsActive
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Courses_Delete",
            new { CourseID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
    public async Task<List<Course>> GetByTeacherIdAsync(int teacherId)
    {
        using var con = _factory.Create();
        var result = await con.QueryAsync<Course>(
            "SELECT * FROM Courses WHERE TeacherID = @TeacherID",
            new { TeacherID = teacherId });
        return result.ToList();
    }

    public async Task<List<User>> GetEnrolledStudentsAsync(int courseId)
    {
        using var con = _factory.Create();
        var result = await con.QueryAsync<User>(
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
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Courses WHERE IsActive = 1");
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
    public async Task<IEnumerable<dynamic>> GetCourseStatsByTeacherAsync(int teacherId)
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync(
            "dbo.sp_TeacherDashboard_GetCourseStats",
            new { TeacherId = teacherId },
            commandType: System.Data.CommandType.StoredProcedure);
    }


}
