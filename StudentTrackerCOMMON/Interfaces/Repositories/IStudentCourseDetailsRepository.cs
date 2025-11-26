using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IStudentCourseDetailsRepository
    {
        Task<StudentCourseDetailsDTO> GetCourseDetailsAsync(int studentId, int courseId);
    }
}
