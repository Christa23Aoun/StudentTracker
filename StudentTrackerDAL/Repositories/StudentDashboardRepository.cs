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
                ?? throw new ArgumentNullException(nameof(config), "Missing database connection string");
        }

        // ============================
        // 1. OVERVIEW
        // ============================
        public async Task<StudentOverviewDTO?> GetOverviewAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryFirstOrDefaultAsync<StudentOverviewDTO>(
                "dbo.sp_StudentDashboard_GetOverview",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }

        // ============================
        // 2. COURSES
        // ============================
        public async Task<IEnumerable<CourseItemDTO>> GetCoursesAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<CourseItemDTO>(
                "dbo.sp_StudentDashboard_GetCourses",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }

        // ============================
        // 3. NOTIFICATIONS (10 latest)
        // ============================
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
                    Title = "", // your DB has no Title column
                    Message = n.Message ?? "",
                    Type = n.Type ?? "info",
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead ?? false
                });
            }

            return list.OrderByDescending(x => x.CreatedAt);
        }

        // ============================
        // 4. GRADE PROGRESS
        // ============================
        public async Task<IEnumerable<GradePointDTO>> GetGradeProgressAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<GradePointDTO>(
                "dbo.sp_StudentDashboard_GetGradeProgress",
                new { StudentID = studentId, Top = 10 },
                commandType: CommandType.StoredProcedure
            );
        }

        // ============================
        // 5. ATTENDANCE TREND
        // ============================
        public async Task<IEnumerable<AttendancePointDTO>> GetAttendanceTrendAsync(int studentId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryAsync<AttendancePointDTO>(
                "dbo.sp_StudentDashboard_GetAttendanceTrend",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
