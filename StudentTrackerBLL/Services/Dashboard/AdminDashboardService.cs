using StudentTrackerCOMMON.DTOs.AdminDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
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
        private readonly IAttendanceRepository _attendance;

        public AdminDashboardService(
            IUserRepository users,
            ICourseRepository courses,
            IDepartmentRepository departments,
            ITestGradeRepository testGrades,
            IAttendanceRepository attendance)
        {
            _users = users;
            _courses = courses;
            _departments = departments;
            _testGrades = testGrades;
            _attendance = attendance;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var students = await _users.CountByRoleAsync("Student");
            var teachers = await _users.CountByRoleAsync("Teacher");
            var courses = await _courses.GetAllAsync();
            var depts = await _departments.GetAllAsync();

            var summary = new AdminDashboardSummaryDto
            {
                TotalStudents = students,
                TotalTeachers = teachers,
                ActiveCourses = courses.Count(),
                Departments = depts.Count(),
                CurrentAcademicYear = "2024-2025",
                CurrentSemester = "Fall"
            };

            return new AdminDashboardDto
            {
                Summary = summary,
                Departments = depts.Select(d => new DepartmentDashboardDto
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
                    DepartmentName = depts.FirstOrDefault(d => d.DepartmentID == c.DepartmentID)?.DepartmentName ?? "—",
                    TeacherName = "—",
                    IsActive = c.IsActive
                }).ToList(),
                Users = (await _users.GetAllAsync()).Select(u => new UserDashboardDto
                {
                    UserID = u.UserID,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.RoleID == 1 ? "Admin" :
                               u.RoleID == 2 ? "Teacher" :
                               u.RoleID == 3 ? "Student" : "Unknown",
                    IsActive = u.IsActive
                }).ToList(),
                PendingGrades = new List<AdminPendingGradeDto>() // ✅ Fixed type
            };
        }
    }
}
