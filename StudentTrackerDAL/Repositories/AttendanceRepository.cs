using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using System.Data;

namespace StudentTrackerDAL.Repositories
{
    public class AttendanceRepository
    {
        private readonly string _connectionString;

        public AttendanceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(Attendance att)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                att.StudentID,
                att.CourseID,
                att.AttendanceDate,
                att.IsPresent
            };
            return await con.ExecuteAsync("sp_CreateAttendance", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Attendance>("sp_GetAllAttendance", commandType: CommandType.StoredProcedure);
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<Attendance>("sp_GetAttendanceByID", new { AttendanceID = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(Attendance att)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new
            {
                att.AttendanceID,
                att.IsPresent,
                att.IsValidated
            };
            return await con.ExecuteAsync("sp_UpdateAttendance", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync("sp_DeleteAttendance", new { AttendanceID = id }, commandType: CommandType.StoredProcedure);
        }
    }
}
