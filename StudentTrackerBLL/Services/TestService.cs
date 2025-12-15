using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services
{
    public class TestService
    {
        private readonly ITestRepository _testRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentCourseService _studentCourseService;

        public TestService(
            ITestRepository testRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IStudentCourseService studentCourseService
        )
        {
            _testRepository = testRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _studentCourseService = studentCourseService;
        }

        public Task<IEnumerable<TestDto>> GetAllAsync()
            => _testRepository.GetAllAsync();

        public Task<TestDto?> GetByIdAsync(int id)
            => _testRepository.GetByIdAsync(id);

        public Task<IEnumerable<TestDto>> GetByCourseIdAsync(int courseId)
            => _testRepository.GetByCourseIdAsync(courseId);

        public async Task<int> CreateAsync(Test t)
        {
            if (string.IsNullOrWhiteSpace(t.TestName))
                throw new ArgumentException("Test name required.");

            if (t.Weight <= 0)
                throw new ArgumentException("Weight must be positive.");

            decimal total = await GetTotalWeightForCourseAsync(t.CourseID);

            if (total + t.Weight > 100)
                throw new ArgumentException("Total test weights cannot exceed 100%.");

            var testId = await _testRepository.CreateAsync(t);

            var teacherId = await _testRepository.GetTeacherIdByCourseAsync(t.CourseID);
            var teacherUser = await _userRepository.GetByIdAsync(teacherId);
            var course = await _courseRepository.GetByIdAsync(t.CourseID);

            var teacherName = teacherUser?.FullName ?? "The teacher";
            var courseName = course?.CourseName ?? "the course";

            var message =
                $"{teacherName} just added a test {t.TestName} for {courseName} scheduled on {t.TestDate:dd/MM/yyyy}";

            var enrollments = await _studentCourseService.GetByCourseAsync(t.CourseID);
            var studentIds = enrollments.Select(e => e.StudentID).Distinct().ToList();

            if (studentIds.Any())
                await _notificationService.NotifyStudentsAsync(
                    studentIds,
                    message,
                    "TEST",
                    $"/Tests?courseId={t.CourseID}"
                );

            if (teacherId > 0)
                await _notificationService.NotifyTeacherAsync(
                    teacherId,
                    $"You added a new test '{t.TestName}' for {courseName}.",
                    "TEST",
                    $"/Tests?courseId={t.CourseID}"
                );

            return testId;
        }

        public async Task<int> UpdateAsync(Test t)
        {
            if (t.Weight <= 0)
                throw new ArgumentException("Weight must be positive.");

            var oldTest = await _testRepository.GetByIdAsync(t.TestID);
            if (oldTest == null)
                throw new ArgumentException("Test not found.");

            decimal totalExisting = await GetTotalWeightForCourseAsync(t.CourseID);
            decimal totalWithoutOld = totalExisting - oldTest.Weight;

            if (totalWithoutOld + t.Weight > 100)
                throw new ArgumentException("Total test weights cannot exceed 100%.");

            return await _testRepository.UpdateAsync(t);
        }

        public Task<int> DeleteAsync(int id)
            => _testRepository.DeleteAsync(id);

        private async Task<decimal> GetTotalWeightForCourseAsync(int courseId)
        {
            var list = await _testRepository.GetByCourseIdAsync(courseId);
            return list.Sum(t => (decimal)t.Weight);
        }
    }
}
