namespace StudentTracker.Models
{
    public class TestView
    {
        public int TestID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }

        public string TestName { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        public double Weight { get; set; }
        public double MaxScore { get; set; }
    }
}
