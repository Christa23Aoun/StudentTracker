using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;
    public CourseService(ICourseRepository repo) => _repo = repo;

    public Task<IEnumerable<CourseListItem>> GetAllAsync() => _repo.GetAllAsync();
    public Task<CourseListItem?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public async Task<IEnumerable<dynamic>> GetCourseStatsByTeacherAsync(int teacherId)
    {
        return await _repo.GetCourseStatsByTeacherAsync(teacherId);
    }

    public Task<int> CreateAsync(CourseCreateDto dto)
    {
        Validate(dto.CourseName, dto.CreditHours, dto.DepartmentID, dto.SemesterID, dto.TeacherID);
        var entity = new Course
        {
            CourseCode = dto.CourseCode,
            CourseName = dto.CourseName.Trim(),
            CreditHours = dto.CreditHours,
            DepartmentID = dto.DepartmentID,
            TeacherID = dto.TeacherID,
            SemesterID = dto.SemesterID,
            IsActive = dto.IsActive
        };
        return _repo.CreateAsync(entity);
    }

    public Task<int> UpdateAsync(CourseUpdateDto dto)
    {
        Validate(dto.CourseName, dto.CreditHours, dto.DepartmentID, dto.SemesterID, dto.TeacherID);
        var entity = new Course
        {
            CourseID = dto.CourseID,
            CourseCode = dto.CourseCode,
            CourseName = dto.CourseName.Trim(),
            CreditHours = dto.CreditHours,
            DepartmentID = dto.DepartmentID,
            TeacherID = dto.TeacherID,
            SemesterID = dto.SemesterID,
            IsActive = dto.IsActive
        };
        return _repo.UpdateAsync(entity);
    }

    public Task<int> DeleteAsync(int id) => _repo.DeleteAsync(id);

    private static void Validate(string name, int creditHours, int deptId, int semId, int teacherId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("CourseName is required.");
        if (creditHours <= 0 || creditHours > 10) throw new ArgumentException("CreditHours must be between 1 and 10.");
        if (deptId <= 0) throw new ArgumentException("DepartmentID is required.");
        if (semId <= 0) throw new ArgumentException("SemesterID is required.");
        if (teacherId <= 0) throw new ArgumentException("TeacherID is required (enter a valid UserID for a teacher).");
    }
    public async Task<IEnumerable<CourseListItem>> GetByTeacherIdAsync(int teacherId)
    {
        var courses = await _repo.GetByTeacherIdAsync(teacherId);

        // Convert List<Course> → IEnumerable<CourseListItem> if necessary
        return courses.Select(c => new CourseListItem
        {
            CourseID = c.CourseID,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            CreditHours = c.CreditHours,
            DepartmentID = c.DepartmentID,
            TeacherID = c.TeacherID,
            SemesterID = c.SemesterID,
            IsActive = c.IsActive
        });
    }



}
