using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IEnrollmentRepository
    {
        Task<IEnumerable<StudentCourse>> GetCoursesByStudentAsync(int studentId);
        Task<IEnumerable<User>> GetStudentsByCourseAsync(int courseId);

        Task<int> EnrollAsync(int studentId, int courseId);
        Task<int> UnenrollAsync(int studentId, int courseId);
        Task<int> BulkEnrollAsync(int studentId, IEnumerable<int> courseIds);
    }
}
