using StudentTrackerCOMMON.Models;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface ICourseSessionService
    {
        Task<IEnumerable<CourseSession>> GetByCourseAsync(int courseId);

        Task<bool> GenerateSessionsAsync(GenerateSessionsRequest request);
        Task<bool> DeleteSessionAsync(int sessionId);
    }
}
