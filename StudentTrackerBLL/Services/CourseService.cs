using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
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

            var changes = new List<string>();

            if (oldCourse.CourseName != dto.CourseName)
                changes.Add("course name");

            if (oldCourse.CreditHours != dto.CreditHours)
                changes.Add("credit hours");

            if (oldCourse.SemesterID != dto.SemesterID)
                changes.Add("semester");

            if (oldCourse.DepartmentID != dto.DepartmentID)
                changes.Add("department");

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

            if (result <= 0)
                return result;

            if (oldCourse.TeacherID != dto.TeacherID)
            {
                await _notificationService.NotifyTeacherAsync(
                    oldCourse.TeacherID,
                    $"You have been unassigned from the course \"{oldCourse.CourseName}\".",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );

                await _notificationService.NotifyTeacherAsync(
                    dto.TeacherID,
                    $"You have been assigned to teach the course \"{entity.CourseName}\".",
                    "ADMIN",
                    "/Teacher/Dashboard"
                );
            }
            else
            {
                string message;

                if (changes.Count == 0)
                    message = $"Course \"{entity.CourseName}\" details have been updated.";
                else if (changes.Count == 1)
                    message = $"Course \"{entity.CourseName}\" {changes[0]} has been updated.";
                else
                    message = $"Course \"{entity.CourseName}\" was updated ({string.Join(", ", changes)}).";

                await _notificationService.NotifyTeacherAsync(
                    dto.TeacherID,
                    message,
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

        public async Task<bool> DeactivateAsync(int id)
        {
            var course = await _repo.GetByIdAsync(id);
            if (course == null)
                return false;

            var rows = await _repo.DeactivateAsync(id);
            if (rows <= 0)
                return false;

            await _notificationService.NotifyTeacherAsync(
                course.TeacherID,
                $"Course \"{course.CourseName}\" has been deactivated by the administration.",
                "ADMIN",
                "/Teacher/Dashboard"
            );

            var students = await _enrollmentRepository.GetStudentsByCourseAsync(id);

            foreach (var s in students)
            {
                await _notificationService.NotifyStudentAsync(
                    s.UserID,
                    $"Course \"{course.CourseName}\" has been deactivated by the administration.",
                    "ADMIN",
                    "/StudentDashboard"
                );
            }

            return true;
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
}
