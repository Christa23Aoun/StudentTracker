namespace StudentTracker.Models
{
    public class TestGradeView
    {
        public int GradeID { get; set; }
        public int TestID { get; set; }
        public int StudentID { get; set; }
        public double Score { get; set; }   // ✅ renamed for clarity & API consistency
        public bool IsValidated { get; set; }  // ✅ add this if your API supports it
    }
}
