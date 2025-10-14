using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories;

public class AcademicYearRepository : IAcademicYearRepository
{
    private readonly ISqlConnectionFactory _factory;
    public AcademicYearRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<AcademicYear>> GetAllAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<AcademicYear>(
            "dbo.AcademicYears_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<AcademicYear?> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.QueryFirstOrDefaultAsync<AcademicYear>(
            "dbo.AcademicYears_GetById",
            new { AcademicYearID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(string name, DateTime startDate, DateTime endDate)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.AcademicYears_Create",
            new { Name = name, StartDate = startDate, EndDate = endDate },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(int id, string name, DateTime startDate, DateTime endDate)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.AcademicYears_Update",
            new { AcademicYearID = id, Name = name, StartDate = startDate, EndDate = endDate },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.AcademicYears_Delete",
            new { AcademicYearID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
