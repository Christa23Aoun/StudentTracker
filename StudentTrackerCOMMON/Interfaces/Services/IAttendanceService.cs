using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetBySessionIdAsync(int sessionId);
        Task<int> CreateAsync(Attendance attendance);
        Task<int> UpdateAsync(Attendance attendance);
        Task<int> DeleteAsync(int attendanceId);
        Task<IEnumerable<int>> GetSessionIdsWithAttendanceByCourseAsync(int courseId, DateTime startDate, DateTime endDate);

    }
}
