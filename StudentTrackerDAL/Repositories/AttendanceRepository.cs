using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(Attendance att)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteScalarAsync<int>(
                "sp_CreateAttendance",
                new
                {
                    att.StudentID,
                    att.CourseID,
                    att.AttendanceDate,
                    att.IsPresent,
                    att.IsValidated
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Attendance>(
                "sp_GetAllAttendance",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<Attendance>(
                "sp_GetAttendanceByID",
                new { AttendanceID = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(Attendance att)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_UpdateAttendance",
                new
                {
                    att.AttendanceID,
                    att.IsPresent,
                    att.IsValidated
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_DeleteAttendance",
                new { AttendanceID = id },
                commandType: CommandType.StoredProcedure);
        }

        // ✅ Add this new method
        public async Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Attendance>(
                "SELECT * FROM Attendance WHERE CourseID = @CourseID",
                new { CourseID = courseId });
        }

        // ✅ Add this new method
        public async Task<decimal> GetAverageAttendanceByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.ExecuteScalarAsync<decimal?>(
                "SELECT AVG(CAST(IsPresent AS DECIMAL(5,2))) FROM Attendance WHERE CourseID = @CourseID",
                new { CourseID = courseId });

            return result ?? 0;
        }
    }
}
