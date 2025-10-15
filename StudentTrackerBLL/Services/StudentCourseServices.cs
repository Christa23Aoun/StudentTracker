using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseService
    {
        private readonly StudentCourseRepository _repository;

        public StudentCourseService(string connectionString)
        {
            _repository = new StudentCourseRepository(connectionString);
        }

        public async Task<IEnumerable<StudentCourse>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<StudentCourse?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(StudentCourse sc)
        {
            if (sc.StudentID <= 0 || sc.CourseID <= 0)
                throw new ArgumentException("Invalid student or course ID.");

            return await _repository.CreateStudentCourseAsync(sc);
        }

        public async Task<int> UpdateAsync(StudentCourse sc)
        {
            return await _repository.UpdateAsync(sc);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
