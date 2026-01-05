namespace StudentTracker.Models
{
    public class AdminExcessiveAbsenceView
    {
        public int StudentID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalAbsences { get; set; }
    }
}
