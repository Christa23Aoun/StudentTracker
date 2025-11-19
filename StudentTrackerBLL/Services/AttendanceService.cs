using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;
using Dapper;
using Microsoft.Data.SqlClient;

namespace StudentTrackerBLL.Services
{
    public class AttendanceService
    {
        private readonly AttendanceRepository _repository;
        private readonly string _connectionString;

        public AttendanceService(string connectionString)
        {
            _connectionString = connectionString;
            _repository = new AttendanceRepository(connectionString);
        }

        public Task<IEnumerable<Attendance>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Attendance?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<int> CreateAsync(Attendance att) => _repository.CreateAsync(att);
        public Task<int> UpdateAsync(Attendance att) => _repository.UpdateAsync(att);
        public Task<int> DeleteAsync(int id) => _repository.DeleteAsync(id);

        // ✔ FIXED: Removed IsValidated
        // ✔ FIXED: Mapped StudentName + CourseName correctly
        // ✔ FIXED: Matches your DB structure perfectly
        public async Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId)
        {
            using var con = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    a.AttendanceID,
                    a.StudentID,
                    u.FullName AS StudentName,
                    a.CourseID,
                    c.CourseName,
                    a.AttendanceDate,
                    a.IsPresent
                FROM Attendance a
                JOIN Users u ON a.StudentID = u.UserID
                JOIN Courses c ON a.CourseID = c.CourseID
                WHERE a.CourseID = @CourseID
                ORDER BY a.AttendanceDate DESC";

            return await con.QueryAsync<Attendance>(sql, new { CourseID = courseId });
        }
    }
}
