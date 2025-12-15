//using System.Threading.Tasks;
//using System.Collections.Generic;
//using StudentTrackerCOMMON.Models;

//namespace StudentTrackerCOMMON.Interfaces.Repositories
//{
//    public interface IAttendanceRepository
//    {
//        Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId);
//        Task<decimal> GetAverageAttendanceByCourseAsync(int courseId);
//    }
//}
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<int>> GetSessionIdsWithAttendanceByCourseAsync(int courseId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<Attendance>> GetBySessionIdAsync(int sessionId);
        Task<bool> ExistsAsync(int studentId, int sessionId);
        Task<int> CreateAsync(Attendance attendance);
        Task<int> UpdateAsync(Attendance attendance);
        Task<int> DeleteAsync(int attendanceId);
    }
}
