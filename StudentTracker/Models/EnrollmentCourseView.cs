namespace StudentTracker.Models
{
    public class EnrollmentCourseView
    {
        public int StudentCourseID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DroppedAt { get; set; }
    }
}
