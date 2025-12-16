using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;
    private readonly INotificationService _notificationService;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public CourseService(
        ICourseRepository repo,
        INotificationService notificationService,
        IEnrollmentRepository enrollmentRepository)
    {
        _repo = repo;
        _notificationService = notificationService;
        _enrollmentRepository = enrollmentRepository;
    }

    public Task<IEnumerable<CourseListItem>> GetAllAsync()
        => _repo.GetAllAsync();

    public Task<CourseListItem?> GetByIdAsync(int id)
        => _repo.GetByIdAsync(id);

    public async Task<IEnumerable<dynamic>> GetCourseStatsByTeacherAsync(int teacherId)
        => await _repo.GetCourseStatsByTeacherAsync(teacherId);

    public async Task<int> CreateAsync(CourseCreateDto dto)
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

        var courseId = await _repo.CreateAsync(entity);

        await _notificationService.NotifyTeacherAsync(
            dto.TeacherID,
            $"You have been assigned to teach the course \"{entity.CourseName}\".",
            "ADMIN",
            "/Teacher/Dashboard"
        );

        return courseId;
    }

    public async Task<int> UpdateAsync(CourseUpdateDto dto)
    {
        Validate(dto.CourseName, dto.CreditHours, dto.DepartmentID, dto.SemesterID, dto.TeacherID);

        var oldCourse = await _repo.GetByIdAsync(dto.CourseID);
        if (oldCourse == null)
            throw new ArgumentException("Course not found.");

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

        var result = await _repo.UpdateAsync(entity);

        if (oldCourse.TeacherID != dto.TeacherID)
        {
            await _notificationService.NotifyTeacherAsync(
                dto.TeacherID,
                $"You have been assigned to teach the course \"{entity.CourseName}\".",
                "ADMIN",
                "/Teacher/Dashboard"
            );
        }
        else
        {
            await _notificationService.NotifyTeacherAsync(
                dto.TeacherID,
                $"Course \"{entity.CourseName}\" details have been updated by the administration.",
                "ADMIN",
                "/Teacher/Dashboard"
            );
        }

        return result;
    }

    public async Task<int> DeleteAsync(int id)
    {
        var course = await _repo.GetByIdAsync(id);
        if (course == null)
            return 0;

        var rows = await _repo.DeleteAsync(id);

        if (rows > 0)
        {
            await _notificationService.NotifyTeacherAsync(
                course.TeacherID,
                $"Course \"{course.CourseName}\" has been removed by the administration.",
                "ADMIN",
                "/Teacher/Dashboard"
            );

            var students = await _enrollmentRepository.GetStudentsByCourseAsync(id);

            foreach (var s in students)
            {
                await _notificationService.NotifyStudentAsync(
                    s.UserID,
                    $"Course \"{course.CourseName}\" has been removed by the administration.",
                    "ADMIN",
                    "/StudentDashboard"
                );
            }
        }

        return rows;
    }

    public async Task<IEnumerable<CourseListItem>> GetByTeacherIdAsync(int teacherId)
    {
        var courses = await _repo.GetByTeacherIdAsync(teacherId);

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

    private static void Validate(string name, int creditHours, int deptId, int semId, int teacherId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("CourseName is required.");

        if (creditHours <= 0 || creditHours > 10)
            throw new ArgumentException("CreditHours must be between 1 and 10.");

        if (deptId <= 0)
            throw new ArgumentException("DepartmentID is required.");

        if (semId <= 0)
            throw new ArgumentException("SemesterID is required.");

        if (teacherId <= 0)
            throw new ArgumentException("TeacherID is required.");
    }
}
