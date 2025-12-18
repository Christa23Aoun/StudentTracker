using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _users;
        private readonly ICourseRepository _courses;
        private readonly IDepartmentRepository _departments;
        private readonly ITestGradeRepository _testGrades;
        private readonly IAdminDashboardRepository _adminDashboard;

        public AdminDashboardService(
            IUserRepository users,
            ICourseRepository courses,
            IDepartmentRepository departments,
            ITestGradeRepository testGrades,
            IAdminDashboardRepository adminDashboard)
        {
            _users = users;
            _courses = courses;
            _departments = departments;
            _testGrades = testGrades;
            _adminDashboard = adminDashboard;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var students = await _users.CountByRoleAsync("Student");
            var teachers = await _users.CountByRoleAsync("Teacher");

            var courses = await _courses.GetAllAsync();
            var departments = await _departments.GetAllAsync();

            // ✅ FIX: use AdminDashboardRepository (single source of truth)
            var pendingGrades = await _adminDashboard.CountPendingGradesAsync();

            var summary = new AdminDashboardSummaryDto
            {
                TotalStudents = students,
                TotalTeachers = teachers,
                ActiveCourses = courses.Count(c => c.IsActive),
                Departments = departments.Count(),
                CurrentAcademicYear = "2024-2025",
                CurrentSemester = "Fall",
                PendingGrades = pendingGrades
            };

            return new AdminDashboardDto
            {
                Summary = summary,

                Departments = departments.Select(d => new DepartmentDashboardDto
                {
                    DepartmentID = d.DepartmentID,
                    DepartmentName = d.DepartmentName,
                    CourseCount = courses.Count(c => c.DepartmentID == d.DepartmentID)
                }).ToList(),

                Courses = courses.Select(c => new CourseDashboardDto
                {
                    CourseID = c.CourseID,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    DepartmentName = departments
                        .FirstOrDefault(d => d.DepartmentID == c.DepartmentID)?.DepartmentName ?? "—",
                    TeacherName = "—",
                    IsActive = c.IsActive
                }).ToList(),

                Users = (await _users.GetAllAsync()).Select(u => new UserDashboardDto
                {
                    UserID = u.UserID,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.RoleID == 1 ? "Admin"
                             : u.RoleID == 2 ? "Teacher"
                             : u.RoleID == 3 ? "Student"
                             : "Unknown",
                    IsActive = u.IsActive
                }).ToList(),

                PendingGrades = new List<AdminPendingGradeItemDto>() // kept as requested
            };
        }

        public async Task<IEnumerable<AdminPendingGradeItemDto>> GetPendingGradesAsync()
        {
            var grades = await _testGrades.GetPendingGradesAsync();

            return grades.Select(g => new AdminPendingGradeItemDto
            {
                TestGradeID = g.TestGradeID,
                TestID = g.TestID,
                TestName = g.TestName,
                CourseID = g.CourseID,
                CourseName = g.CourseName,
                StudentID = g.StudentID,
                StudentName = g.StudentName,
                Score = g.Score,
                CreatedAt = g.CreatedAt
            });
        }

        public async Task<bool> ValidateGradeAsync(int testGradeId)
        {
            var affected = await _testGrades.MarkGradeAsValidatedAsync(testGradeId);
            return affected > 0;
        }

        public async Task<bool> RejectGradeAsync(int testGradeId)
        {
            var affected = await _testGrades.DeleteGradeAsync(testGradeId);
            return affected > 0;
        }
    }
}
