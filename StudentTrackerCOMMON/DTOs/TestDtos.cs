namespace StudentTrackerCOMMON.DTOs
{
    public class TestDto
    {
        public int TestID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public int Weight { get; set; }
        public int MaxScore { get; set; }
    }

    public class CreateTestRequest
    {
        public int CourseID { get; set; }
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public int Weight { get; set; }
        public int MaxScore { get; set; }
    }

    public class UpdateTestRequest
    {
        public int TestID { get; set; }
        public int CourseID { get; set; }
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public int Weight { get; set; }
        public int MaxScore { get; set; }
    }
}
