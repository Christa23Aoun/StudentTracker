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

        public async Task<Department?> GetByIdAsync(int id)
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
            WHERE d.DepartmentID = @id
            GROUP BY d.DepartmentID, d.DepartmentName, d.CreatedAt, d.Description, d.IsActive;";

            return await conn.QueryFirstOrDefaultAsync<Department>(sql, new { id });
        }

        public async Task<int> CreateAsync(string departmentName)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Create",
                new { DepartmentName = departmentName },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(int id, string departmentName, bool isActive)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Update",
                new
                {
                    DepartmentID = id,
                    DepartmentName = departmentName,
                    Description = (string?)null,
                    IsActive = isActive
                },
                commandType: CommandType.StoredProcedure);
        }



        public async Task<int> DeleteAsync(int id)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Delete",
                new { DepartmentID = id },
                commandType: CommandType.StoredProcedure);
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

        public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(int departmentId)
        {
            using var conn = _factory.Create();

            var sql = @"
            SELECT CourseID, CourseName
            FROM Courses
            WHERE DepartmentID = @departmentId";

            return await conn.QueryAsync<Course>(sql, new { departmentId });
        }
        public async Task<int> UpdateStatusAsync(int id, bool isActive)
        {
            using var conn = _factory.Create();

            return await conn.ExecuteScalarAsync<int>(
                "dbo.Departments_Update",
                new
                {
                    DepartmentID = id,
                    DepartmentName = "",
                    Description = (string?)null,
                    IsActive = isActive
                },
                commandType: CommandType.StoredProcedure
            );
        }

    }
}
