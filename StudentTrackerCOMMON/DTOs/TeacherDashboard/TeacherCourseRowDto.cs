namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class TeacherCourseRowDto
    {
        public int CourseID { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }
    }
}
