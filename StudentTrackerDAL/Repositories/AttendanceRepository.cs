using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System;
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

        public async Task<bool> ExistsAsync(int studentId, int sessionId)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.ExecuteScalarAsync<int>(
                "sp_AttendanceExists_BySession",
                new { StudentID = studentId, SessionID = sessionId },
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<int> CreateAsync(Attendance attendance)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            using var tx = con.BeginTransaction(IsolationLevel.Serializable);

            var exists = await con.ExecuteScalarAsync<int>(
                "sp_AttendanceExists_BySession",
                new { StudentID = attendance.StudentID, SessionID = attendance.SessionID },
                transaction: tx,
                commandType: CommandType.StoredProcedure);

            if (exists > 0)
            {
                tx.Rollback();
                throw new InvalidOperationException("Attendance already exists.");
            }

            var id = await con.ExecuteScalarAsync<int>(
                "sp_CreateAttendance_BySession",
                new
                {
                    StudentID = attendance.StudentID,
                    SessionID = attendance.SessionID,
                    CourseID = attendance.CourseID,
                    IsPresent = attendance.IsPresent
                },
                transaction: tx,
                commandType: CommandType.StoredProcedure);

            tx.Commit();
            return id;
        }

        public async Task<double> GetAttendanceRateByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    CAST(
                        (100.0 * SUM(CASE WHEN IsPresent = 1 THEN 1 ELSE 0))
                        / NULLIF(COUNT(*), 0)
                    AS DECIMAL(5,2))
                FROM Attendance
                WHERE CourseID = @CourseID";

            return await con.ExecuteScalarAsync<double>(sql, new { CourseID = courseId });
        }

        public async Task<int> DeleteAsync(int attendanceId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_DeleteAttendance",
                new { AttendanceID = attendanceId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Attendance>> GetBySessionIdAsync(int sessionId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Attendance>(
                "sp_GetAttendanceBySession",
                new { SessionID = sessionId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<int>> GetSessionIdsWithAttendanceByCourseAsync(
            int courseId,
            DateTime startDate,
            DateTime endDate)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                SELECT DISTINCT SessionID
                FROM Attendance
                WHERE CourseID = @CourseID
                  AND CAST(CreatedAt AS DATE) BETWEEN @StartDate AND @EndDate
            ";

            return await con.QueryAsync<int>(
                sql,
                new
                {
                    CourseID = courseId,
                    StartDate = startDate.Date,
                    EndDate = endDate.Date
                });
        }

        public async Task<int> UpdateAsync(Attendance attendance)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteAsync(
                "sp_UpdateAttendance",
                new
                {
                    attendance.AttendanceID,
                    attendance.IsPresent
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
