using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ICourseSessionRepository
    {
        Task<int> CreateAsync(CourseSession session);
        Task<CourseSession?> GetByIdAsync(int sessionId);
        Task<IEnumerable<CourseSession>> GetByCourseAsync(int courseId);
        Task<int> UpdateAsync(CourseSession session);
        Task<int> DeleteAsync(int sessionId);
    }
}
