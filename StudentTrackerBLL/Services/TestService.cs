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
        private readonly ITestRepository _repository;
        private readonly INotificationService _notificationService;

        public TestService(
            ITestRepository repository,
            INotificationService notificationService
        )
        {
            _repository = repository;
            _notificationService = notificationService;
        }

        public Task<IEnumerable<TestDto>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<TestDto?> GetByIdAsync(int id)
            => _repository.GetByIdAsync(id);

        public Task<IEnumerable<TestDto>> GetByCourseIdAsync(int courseId)
            => _repository.GetByCourseIdAsync(courseId);

        public async Task<int> CreateAsync(Test t)
        {
            if (string.IsNullOrWhiteSpace(t.TestName))
                throw new ArgumentException("Test name required.");

            if (t.Weight <= 0)
                throw new ArgumentException("Weight must be positive.");

            decimal total = await GetTotalWeightForCourseAsync(t.CourseID);

            if (total + t.Weight > 100)
                throw new ArgumentException("Total test weights cannot exceed 100%.");

            var id = await _repository.CreateAsync(t);

            var teacherId = await _repository.GetTeacherIdByCourseAsync(t.CourseID);

            if (teacherId > 0)
            {
                await _notificationService.CreateAsync(new Notification
                {
                    UserID = teacherId,
                    Message = $"A new test '{t.TestName}' has been added.",
                    Type = "TEST"
                });
            }

            return id;
        }

        public async Task<int> UpdateAsync(Test t)
        {
            if (t.Weight <= 0)
                throw new ArgumentException("Weight must be positive.");

            var oldTest = await _repository.GetByIdAsync(t.TestID);
            if (oldTest == null)
                throw new ArgumentException("Test not found.");

            decimal totalExisting = await GetTotalWeightForCourseAsync(t.CourseID);
            decimal totalWithoutOld = totalExisting - oldTest.Weight;

            if (totalWithoutOld + t.Weight > 100)
                throw new ArgumentException("Total test weights cannot exceed 100%.");

            return await _repository.UpdateAsync(t);
        }

        public Task<int> DeleteAsync(int id)
            => _repository.DeleteAsync(id);

        private async Task<decimal> GetTotalWeightForCourseAsync(int courseId)
        {
            var list = await _repository.GetByCourseIdAsync(courseId);
            return list.Sum(t => (decimal)t.Weight);
        }
    }
}
