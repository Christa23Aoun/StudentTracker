using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class TeacherDashboardDto
    {
        // Summary cards
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
        public decimal AverageGrade { get; set; }       // 0–100
        public decimal AttendanceRate { get; set; }     // 0–100

        // Tables/sections
        public List<TeacherCourseRowDto> Courses { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        // Simple chart points (e.g., attendance vs performance)
        public List<SeriesPointDto> AttendanceVsPerformance { get; set; } = new();
    }
}
