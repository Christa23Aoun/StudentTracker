namespace StudentTracker.Models
{
    public class AdminExcellentStudentView
    {
        public int StudentID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal FinalGrade { get; set; }
        public string TestName { get; set; } = string.Empty;

    }
}
