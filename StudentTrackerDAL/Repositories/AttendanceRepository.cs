using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StudentTrackerDAL.Repositories
{
    public class AttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ✅ CREATE
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

        // ✅ READ ALL
        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Attendance>(
                "sp_GetAllAttendance",
                commandType: CommandType.StoredProcedure);
        }

        // ✅ READ ONE
        public async Task<Attendance?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<Attendance>(
                "sp_GetAttendanceByID",
                new { AttendanceID = id },
                commandType: CommandType.StoredProcedure);
        }

        // ✅ UPDATE
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

        // ✅ DELETE
        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_DeleteAttendance",
                new { AttendanceID = id },
                commandType: CommandType.StoredProcedure);
        }
    }
}
