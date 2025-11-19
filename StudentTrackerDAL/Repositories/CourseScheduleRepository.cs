using Dapper;
using Microsoft.Data.SqlClient;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class CourseScheduleRepository : ICourseScheduleRepository
    {
        private readonly string _connectionString;

        public CourseScheduleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateAsync(CourseSchedule schedule)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new
            {
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime
            };

            return await con.ExecuteAsync("sp_CreateCourseSchedule", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateAsync(CourseSchedule schedule)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new
            {
                schedule.ScheduleID,
                schedule.CourseID,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime
            };

            return await con.ExecuteAsync("sp_UpdateCourseSchedule", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteAsync(int scheduleId)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new { ScheduleID = scheduleId };

            return await con.ExecuteAsync("sp_DeleteCourseSchedule", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<CourseSchedule?> GetByIdAsync(int scheduleId)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new { ScheduleID = scheduleId };

            return await con.QueryFirstOrDefaultAsync<CourseSchedule>(
                "SELECT * FROM CourseSchedule WHERE ScheduleID = @ScheduleID",
                parameters
            );
        }

        public async Task<IEnumerable<CourseSchedule>> GetByCourseAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new { CourseID = courseId };

            return await con.QueryAsync<CourseSchedule>(
                "sp_GetScheduleByCourse",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> CheckTeacherConflictAsync(int courseId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new
            {
                CourseID = courseId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };

            var result = await con.QueryFirstOrDefaultAsync<int>(
                @"SELECT CASE 
                       WHEN EXISTS (
                           SELECT 1
                           FROM CourseSchedule cs
                           INNER JOIN Courses c ON c.CourseID = cs.CourseID
                           WHERE c.TeacherID = (SELECT TeacherID FROM Courses WHERE CourseID = @CourseID)
                             AND cs.DayOfWeek = @DayOfWeek
                             AND cs.StartTime < @EndTime
                             AND @StartTime < cs.EndTime
                       ) THEN 1 ELSE 0 END",
                parameters
            );

            return result == 1;
        }

        public async Task<bool> CheckStudentConflictAsync(int studentId, byte dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new
            {
                StudentID = studentId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };

            var result = await con.QueryFirstOrDefaultAsync<int>(
                "sp_CheckStudentScheduleConflict",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result == 1;
        }
    }
}
