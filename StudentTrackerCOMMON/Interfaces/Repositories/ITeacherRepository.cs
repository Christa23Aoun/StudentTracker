using StudentTrackerCOMMON.Models;

namespace StudentTrackerCOMMON.Interfaces.Repositories
{
    public interface ITeacherRepository
    {
        Task<Teacher?> GetByEmailAsync(string email);
    }
}
