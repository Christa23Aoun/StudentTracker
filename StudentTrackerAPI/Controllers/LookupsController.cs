using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        private readonly IDepartmentRepository _departments;
        private readonly ISemesterRepository _semesters;
        private readonly IUserRepository _users;

        public LookupsController(
            IDepartmentRepository departments,
            ISemesterRepository semesters,
            IUserRepository users)
        {
            _departments = departments;
            _semesters = semesters;
            _users = users;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var list = await _departments.GetAllAsync();

            var result = list.Select(d => new
            {
                DepartmentID = d.DepartmentID,
                Name = d.DepartmentName
            });

            return Ok(result);
        }

        [HttpGet("semesters")]
        public async Task<IActionResult> GetSemesters()
        {
            var list = await _semesters.GetAllAsync();

            var result = list.Select(s => new
            {
                SemesterID = s.SemesterID,
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            });

            return Ok(result);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var list = await _users.GetAllAsync();

            var result = list
                .Where(u => u.RoleID == 2 && u.IsActive)
                .Select(u => new
                {
                    UserID = u.UserID,
                    Name = u.FullName
                });

            return Ok(result);
        }
    }
}
