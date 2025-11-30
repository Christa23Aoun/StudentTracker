namespace StudentTracker.Models
{
    public class TestGradeView
    {
        // Shown in views only – OK
        public string StudentName { get; set; }

        // MUST match API
        public int TestGradeID { get; set; }
        public int TestID { get; set; }
        public int StudentID { get; set; }
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }

        // Optional fields for the UI only (MVC will ignore sending them)
        public string TestName { get; set; }
        public string CourseName { get; set; }
        public int CourseID { get; set; }
    }
}
