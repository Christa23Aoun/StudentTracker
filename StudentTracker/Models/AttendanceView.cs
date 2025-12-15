namespace StudentTracker.Models
{
    public class AttendanceView
    {
        public int AttendanceID { get; set; }

        public int StudentID { get; set; }

        public int CourseID { get; set; }

        public int SessionID { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string? StudentName { get; set; }

        public bool IsPresent { get; set; }

        public string Status { get; set; } = "Absent";
    }
}
