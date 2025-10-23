namespace StudentTracker.Models
{
    public class DashboardSummaryViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public string CurrentAcademicYear { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
    }
}
