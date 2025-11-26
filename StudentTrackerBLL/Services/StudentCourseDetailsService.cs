using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerDAL.Repositories;

namespace StudentTrackerBLL.Services
{
    public class StudentCourseDetailsService : IStudentCourseDetailsService
    {
        private readonly IStudentCourseDetailsRepository _repo;

        public StudentCourseDetailsService(IStudentCourseDetailsRepository repo)
        {
            _repo = repo;
        }

        public async Task<StudentCourseDetailsDTO> GetCourseDetailsAsync(int studentId, int courseId)
        {
            return await _repo.GetCourseDetailsAsync(studentId, courseId);
        }
    }
}
