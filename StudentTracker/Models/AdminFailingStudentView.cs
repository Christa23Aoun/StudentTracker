namespace StudentTracker.Models
{
    public class AdminFailingStudentView
    {
        public string FullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public decimal FinalGrade { get; set; }
    }
}
