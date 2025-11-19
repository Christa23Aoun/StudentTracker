namespace StudentTrackerCOMMON.DTOs.AdminDashboard
{
    public class TestGradePendingDto
    {
        public int TestGradeID { get; set; }
        public int TestID { get; set; }
        public string TestName { get; set; } = "";
        public int CourseID { get; set; }
        public string CourseName { get; set; } = "";
        public int StudentID { get; set; }
        public string StudentName { get; set; } = "";
        public decimal Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
