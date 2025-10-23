using System.Data;
using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ISqlConnectionFactory _factory;
    public DepartmentRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<Department>(
            "dbo.Departments_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.QueryFirstOrDefaultAsync<Department>(
            "dbo.Departments_GetById",
            new { DepartmentID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(string departmentName)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Departments_Create",
            new { DepartmentName = departmentName },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(int id, string departmentName)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Departments_Update",
            new { DepartmentID = id, DepartmentName = departmentName },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Departments_Delete",
            new { DepartmentID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
  
    public async Task<int> CountAsync()
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departments");
    }

   
    public async Task<IEnumerable<dynamic>> GetDepartmentSummaryAsync()
    {
        using var conn = _factory.Create();
        var sql = @"
        SELECT d.DepartmentID AS DepartmentId,
               d.DepartmentName,
               COUNT(c.CourseID) AS CourseCount
        FROM Departments d
        LEFT JOIN Courses c ON c.DepartmentID = d.DepartmentID
        GROUP BY d.DepartmentID, d.DepartmentName
        ORDER BY d.DepartmentName;";
        return await conn.QueryAsync(sql);
    }


}
