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

        public StudentCourseService(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            throw new NotSupportedException();
        }

        public Task<StudentCourse?> GetByIdAsync(int id)
        {
            throw new NotSupportedException();
        }

        public Task<int> CreateAsync(StudentCourse model)
        {
            return _enrollmentRepository.EnrollAsync(model.StudentID, model.CourseID);
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
            return rows > 0;
        }
    }
}
