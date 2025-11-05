namespace StudentTracker.Models
{
    public class TeacherCourseRowView
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

    public class RecentActivityView
    {
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class TeacherDashboardView
    {
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmail { get; set; } = string.Empty;
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }

        public List<TeacherCourseRowView> Courses { get; set; } = new();
        public List<RecentActivityView> RecentActivities { get; set; } = new();
    }
}
