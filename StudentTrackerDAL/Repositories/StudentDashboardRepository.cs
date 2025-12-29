using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;

namespace StudentTrackerDAL.Repositories
{
    public class StudentDashboardRepository : IStudentDashboardRepository
    {
        private readonly string _connectionString;

        public StudentDashboardRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<StudentOverviewDTO?> GetOverviewAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryFirstOrDefaultAsync<StudentOverviewDTO>(
                "dbo.sp_StudentDashboard_GetOverview",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<CourseItemDTO>> GetCoursesAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<CourseItemDTO>(
                "dbo.sp_StudentDashboard_GetCourses",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<NotificationDTO>> GetNotificationsAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            var raw = await con.QueryAsync<dynamic>(
                "dbo.sp_StudentDashboard_GetNotifications",
                new { StudentID = studentId, Top = 10 },
                commandType: CommandType.StoredProcedure
            );

            var list = new List<NotificationDTO>();

            foreach (var n in raw)
            {
                list.Add(new NotificationDTO
                {
                    NotificationId = n.NotificationID,
                    Title = "",
                    Message = n.Message ?? "",
                    Type = n.Type ?? "info",
                    CreatedAt = n.CreatedAt is DateTime dt ? dt : DateTime.MinValue,
                    IsRead = n.IsRead is bool b && b
                });
            }

            return list.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<IEnumerable<GradePointDTO>> GetGradeProgressAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<GradePointDTO>(
                "dbo.sp_StudentDashboard_GetGradeProgress",
                new { StudentID = studentId, Top = 10 },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<AttendancePointDTO>> GetAttendanceTrendAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<AttendancePointDTO>(
                "dbo.sp_StudentDashboard_GetAttendanceTrend",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<StudentScheduleItemDto>> GetStudentScheduleAsync(
            int studentId,
            int semesterId,
            DateTime weekStart,
            DateTime weekEnd)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<StudentScheduleItemDto>(
                "dbo.sp_StudentSchedule_GetWeekly",
                new
                {
                    StudentID = studentId,
                    SemesterID = semesterId,
                    WeekStart = weekStart,
                    WeekEnd = weekEnd
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
