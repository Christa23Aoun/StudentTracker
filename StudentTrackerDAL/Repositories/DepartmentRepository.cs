using System.Data;
using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public DepartmentRepository(ISqlConnectionFactory factory) => _factory = factory;

        // ===========================
        // GET ALL
        // ===========================
        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            using var conn = _factory.Create();

            var sql = @"
            SELECT 
                d.DepartmentID,
                d.DepartmentName,
                d.CreatedAt,
                d.Description,
                CAST(ISNULL(d.IsActive, 0) AS BIT) AS IsActive,
                COUNT(c.CourseID) AS CourseCount,
                STRING_AGG(c.CourseName, ', ') AS CourseNames
            FROM Departments d
            LEFT JOIN Courses c ON c.DepartmentID = d.DepartmentID
            GROUP BY d.DepartmentID, d.DepartmentName, d.CreatedAt, d.Description, d.IsActive
            ORDER BY d.DepartmentName;";

            return await conn.QueryAsync<Department>(sql);
        }

        // ===========================
        // GET BY ID
        // ===========================
        public async Task<Department?> GetByIdAsync(int id)
        {
            using var conn = _factory.Create();

            var sql = @"
            SELECT 
                d.DepartmentID,
                d.DepartmentName,
                d.CreatedAt,
                d.Description,
                CAST(ISNULL(d.IsActive, 0) AS BIT) AS IsActive
            FROM Departments d
            WHERE d.DepartmentID = @id;";

            return await conn.QueryFirstOrDefaultAsync<Department>(sql, new { id });
        }

        // ===========================
        // CREATE
        // ===========================
        public async Task<int> CreateAsync(string departmentName)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Create",
                new { DepartmentName = departmentName },
                commandType: CommandType.StoredProcedure);
        }

        // ===========================
        // UPDATE
        // ===========================
        public async Task<int> UpdateAsync(int id, string departmentName)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Update",
                new { DepartmentID = id, DepartmentName = departmentName },
                commandType: CommandType.StoredProcedure);
        }

        // ===========================
        // DELETE
        // ===========================
        public async Task<int> DeleteAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Delete",
                new { DepartmentID = id },
                commandType: CommandType.StoredProcedure);
        }

        // ===========================
        // COUNT
        // ===========================
        public async Task<int> CountAsync()
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departments");
        }

        // ===========================
        // SUMMARY
        // ===========================
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
}
