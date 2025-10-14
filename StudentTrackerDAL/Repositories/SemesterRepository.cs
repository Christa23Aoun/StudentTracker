using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly ISqlConnectionFactory _factory;
    public SemesterRepository(ISqlConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Semester>> GetAllAsync()
    {
        using var conn = _factory.Create();
        return await conn.QueryAsync<Semester>(
            "dbo.Semesters_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Semester?> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.QueryFirstOrDefaultAsync<Semester>(
            "dbo.Semesters_GetById",
            new { SemesterID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(int academicYearID, string name, int semesterNb, DateTime startDate, DateTime endDate)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Semesters_Create",
            new { AcademicYearID = academicYearID, Name = name, SemesterNb = semesterNb, StartDate = startDate, EndDate = endDate },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(int semesterID, int academicYearID, string name, int semesterNb, DateTime startDate, DateTime endDate)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Semesters_Update",
            new { SemesterID = semesterID, AcademicYearID = academicYearID, Name = name, SemesterNb = semesterNb, StartDate = startDate, EndDate = endDate },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> DeleteAsync(int id)
    {
        using var conn = _factory.Create();
        return await conn.ExecuteScalarAsync<int>(
            "dbo.Semesters_Delete",
            new { SemesterID = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
