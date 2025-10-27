using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;

namespace StudentTrackerBLL.Services;

public class DashboardService
{
    private readonly ICourseRepository _courses;
    private readonly IUserRepository _users;
    private readonly IDepartmentRepository _departments;

    public DashboardService(
        ICourseRepository courses,
        IUserRepository users,
        IDepartmentRepository departments)
    {
        _courses = courses;
        _users = users;
        _departments = departments;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
    {
        var students = await _users.CountByRoleAsync("Student");
        var teachers = await _users.CountByRoleAsync("Teacher");
        var courses = await _courses.CountActiveAsync();
        var depts = await _departments.CountAsync();

        return new AdminDashboardSummaryDto(
            TotalStudents: students,
            TotalTeachers: teachers,
            ActiveCourses: courses,
            Departments: depts,
            CurrentAcademicYear: "2024-2025",
            CurrentSemester: "Fall"
        );
    }
    public async Task<IEnumerable<dynamic>> GetDepartmentsAsync()
    {
        return await _departments.GetDepartmentSummaryAsync();
    }
    public async Task<IEnumerable<dynamic>> GetCoursesAsync()
    {
        return await _courses.GetCourseSummaryAsync();
    }


}
