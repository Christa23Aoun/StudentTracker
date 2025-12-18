using System.Collections.Generic;

namespace StudentTrackerCOMMON.DTOs.AdminDashboard
{
    public class AdminFullDashboardDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public int AttendanceCount { get; set; }
        public int TotalCourses { get; set; }
        public int TotalUsers { get; set; }

        public string CurrentAcademicYear { get; set; }
        public string CurrentSemester { get; set; }

        public int PendingGrades { get; set; }

    }
}


