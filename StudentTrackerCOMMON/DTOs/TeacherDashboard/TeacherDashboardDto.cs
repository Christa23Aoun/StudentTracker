using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class TeacherDashboardDto
    {
        
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
        public decimal AverageGrade { get; set; }       // 0–100
        public decimal AttendanceRate { get; set; }     // 0–100

        public List<TeacherCourseRowDto> Courses { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        
        public List<SeriesPointDto> AttendanceVsPerformance { get; set; } = new();
    }
}
