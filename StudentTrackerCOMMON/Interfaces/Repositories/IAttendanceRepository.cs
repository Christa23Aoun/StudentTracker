using System.Threading.Tasks;
using System.Collections.Generic;
using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface IAttendanceRepository
    {
        Task<IEnumerable<Attendance>> GetByCourseIdAsync(int courseId);
        Task<decimal> GetAverageAttendanceByCourseAsync(int courseId);
    }
}
