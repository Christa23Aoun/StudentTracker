namespace StudentTracker.Models
{
    public class AttendanceView
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public DateTime AttendanceDate { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public bool IsPresent { get; set; }

        // Present / Late / Absent
        public string Status { get; set; } = "Absent";

        // Used only for display / binding from form if needed
        public TimeSpan? SessionTime { get; set; }

        public string? StudentName { get; set; }
    }
}
