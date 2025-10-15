using Dapper;
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
}
