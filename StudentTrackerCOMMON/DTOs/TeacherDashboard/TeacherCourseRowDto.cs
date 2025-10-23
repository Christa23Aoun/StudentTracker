using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class TeacherCourseRowDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;

        public int StudentCount { get; set; }

        // Percentages 0–100 (store as decimal to avoid rounding issues)
        public decimal AverageGrade { get; set; }
        public decimal AttendanceRate { get; set; }
    }
}
