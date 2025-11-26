using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IStudentCourseDetailsService
    {
        Task<StudentCourseDetailsDTO> GetCourseDetailsAsync(int studentId, int courseId);
    }
}
