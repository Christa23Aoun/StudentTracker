using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly INotificationService _notificationService;
        private readonly ICourseRepository _courseRepository;

        public StudentCourseService(
            IEnrollmentRepository enrollmentRepository,
            INotificationService notificationService,
            ICourseRepository courseRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _notificationService = notificationService;
            _courseRepository = courseRepository;
        }

        public Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            throw new NotSupportedException();
        }

        public Task<StudentCourse?> GetByIdAsync(int id)
        {
            throw new NotSupportedException();
        }

        public async Task<int> CreateAsync(StudentCourse model)
        {
            var success = await EnrollAsync(model.StudentID, model.CourseID);
            return success ? 1 : 0;
        }

        public Task<int> UpdateAsync(StudentCourse model)
        {
            throw new NotSupportedException();
        }

        public Task<int> DeleteAsync(int id)
        {
            throw new NotSupportedException();
        }

        public Task<IEnumerable<StudentCourse>> GetCoursesByStudentAsync(int userId)
        {
            return _enrollmentRepository.GetCoursesByStudentAsync(userId);
        }

        public async Task<IEnumerable<StudentCourse>> GetByCourseAsync(int courseId)
        {
            var students = await _enrollmentRepository.GetStudentsByCourseAsync(courseId);
            var result = new List<StudentCourse>();

            foreach (var s in students)
            {
                result.Add(new StudentCourse
                {
                    StudentID = s.UserID,
                    CourseID = courseId
                });
            }

            return result;
        }

        public async Task<bool> EnrollAsync(int userId, int courseId)
        {
            var rows = await _enrollmentRepository.EnrollAsync(userId, courseId);
            if (rows <= 0)
                return false;

            var course = await _courseRepository.GetByIdAsync(courseId);
            var courseName = course?.CourseName ?? "your course";

            await _notificationService.NotifyStudentAsync(
                userId,
                $"You have been enrolled in {courseName}.",
                "ADMIN",
                $"/StudentDashboard/CourseDetails?courseId={courseId}"
            );

            return true;
        }
    }
}
