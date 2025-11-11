namespace StudentTracker.Models
{
    public class StudentCourseView
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public double? AverageGrade { get; set; }
        public double? AttendanceRate { get; set; }
        public string StudentName { get; set; } = string.Empty;
    }
}
