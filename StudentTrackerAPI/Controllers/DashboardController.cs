using Microsoft.AspNetCore.Mvc;
using StudentTrackerBLL.Services.Dashboard;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly AdminDashboardService _adminService;
        private readonly StudentDashboardService _studentService; // 👈 new injected service

        public DashboardController(
            AdminDashboardService adminService,
            StudentDashboardService studentService // 👈 inject both safely
        )
        {
            _adminService = adminService;
            _studentService = studentService;
        }

        // === ✅ Existing admin summary (keep exactly as your teammate wrote) ===
        [HttpGet("admin/summary")]
        public async Task<ActionResult> GetSummary()
        {
            var dashboard = await _adminService.GetAdminDashboardAsync();
            return Ok(dashboard);
        }

        // === 🆕 Add Student Dashboard endpoint ===
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult> GetStudentDashboard(int studentId)
        {
            var dashboard = await _studentService.GetStudentDashboardAsync(studentId);
            if (dashboard == null)
                return NotFound($"Student {studentId} not found");

            return Ok(dashboard);
        }
    }
}
