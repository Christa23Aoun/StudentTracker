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
                    att.IsPresent
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

        public async Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
        SELECT 
            a.AttendanceID,
            a.StudentID,
            a.CourseID,
            a.AttendanceDate,
            a.IsPresent,
            u.FullName AS StudentName,
            c.CourseName
        FROM Attendance a
        JOIN Users u ON a.StudentID = u.UserID
        JOIN Courses c ON a.CourseID = c.CourseID
        WHERE a.CourseID = @CourseID
        ORDER BY a.AttendanceDate DESC";

            return await con.QueryAsync<Attendance>(sql, new { CourseID = courseId });
        }

        public async Task<int> UpdateAsync(Attendance att)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_UpdateAttendance",
                new
                {
                    att.AttendanceID,
                    att.IsPresent
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

        public async Task<decimal> GetAverageAttendanceByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.ExecuteScalarAsync<decimal?>(
                "SELECT AVG(CAST(IsPresent AS DECIMAL(5,2))) * 100 FROM Attendance WHERE CourseID = @CourseID",
                new { CourseID = courseId });

            return result ?? 0;
        }
    }
}
