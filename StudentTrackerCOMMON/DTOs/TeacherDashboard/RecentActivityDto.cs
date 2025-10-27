using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }              // when it happened
        public string Type { get; set; } = string.Empty;     // "Test", "Attendance", "Grade"
        public string Description { get; set; } = string.Empty; // human-readable text
    }
}

