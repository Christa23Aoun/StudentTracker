using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseService
    {
        private readonly StudentCourseRepository _repo;

        public StudentCourseService(string connectionString)
        {
            _repo = new StudentCourseRepository(connectionString);
        }

        
        public async Task<IEnumerable<StudentCourse>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<StudentCourse?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task<int> CreateAsync(StudentCourse entity) => await _repo.CreateStudentCourseAsync(entity);

        public async Task<int> UpdateAsync(StudentCourse entity) => await _repo.UpdateAsync(entity);

        public async Task<int> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<IEnumerable<dynamic>> GetCoursesByStudentAsync(int studentId)
        {
            return await _repo.GetAllAsync(); 
        }

        public async Task<bool> EnrollAsync(int studentId, int courseId)
        {
            // Use a stored procedure for enrollment if you have one, or write inline SQL in the DAL
            return true; 
        }
    }
}
