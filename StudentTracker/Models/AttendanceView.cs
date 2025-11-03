namespace StudentTracker.Models
{
    public class AttendanceView
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
    }
}
