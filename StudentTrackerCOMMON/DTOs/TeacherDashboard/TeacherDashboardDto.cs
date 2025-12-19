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
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }

        public List<TeacherCourseRowDto> Courses { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        
        public List<SeriesPointDto> AttendanceVsPerformance { get; set; } = new();
    }
}
