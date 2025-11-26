using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Data;

namespace StudentTrackerDAL.Repositories
{
    public class StudentCourseDetailsRepository : IStudentCourseDetailsRepository
    {
        private readonly string _connectionString;

        public StudentCourseDetailsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<StudentCourseDetailsDTO> GetCourseDetailsAsync(int studentId, int courseId)
        {
            var dto = new StudentCourseDetailsDTO();

            using var con = new SqlConnection(_connectionString);

            // ------------------------------------------------------------
            // 1) SUMMARY
            // ------------------------------------------------------------
            var summary = await con.QueryFirstOrDefaultAsync<dynamic>(
                "dbo.sp_StudentCourse_GetSummary",
                new { StudentID = studentId, CourseID = courseId },
                commandType: CommandType.StoredProcedure);

            if (summary != null)
            {
                dto.CourseId = summary.CourseID ?? courseId;
                dto.CourseName = summary.CourseName ?? "";
                dto.TeacherName = summary.TeacherName ?? "";
                dto.CreditHours = Convert.ToInt32(summary.CreditHours ?? 0);
                dto.AttendanceRate = Convert.ToDouble(summary.AttendanceRate ?? 0);
                dto.CurrentAverage = Convert.ToDouble(summary.CurrentAverage ?? 0);
            }

            // ------------------------------------------------------------
            // 2) GRADES (TestID, TestName, TestDate, MaxScore, Weight, Score, Status)
            // ------------------------------------------------------------
            var grades = await con.QueryAsync<StudentTestGradeItemDTO>(
                "dbo.sp_StudentCourse_GetGrades",
                new { StudentID = studentId, CourseID = courseId },
                commandType: CommandType.StoredProcedure);

            dto.Tests = grades?.ToList() ?? new List<StudentTestGradeItemDTO>();

            // ------------------------------------------------------------
            // 3) ATTENDANCE (DateTime, IsPresent)
            // ------------------------------------------------------------
            var attendance = await con.QueryAsync<StudentAttendanceItemDTO>(
                "dbo.sp_StudentCourse_GetAttendance",
                new { StudentID = studentId, CourseID = courseId },
                commandType: CommandType.StoredProcedure);

            dto.Attendance = attendance?.ToList() ?? new List<StudentAttendanceItemDTO>();

            return dto;
        }
    }
}
