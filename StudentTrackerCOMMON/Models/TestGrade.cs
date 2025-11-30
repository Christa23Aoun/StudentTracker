using System;

namespace StudentTrackerCOMMON.Models
{
    public class TestGrade
    {
        public string? StudentName { get; set; } = null;

        public int TestGradeID { get; set; }
        public int TestID { get; set; }
        public int StudentID { get; set; }
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }
        public DateTime? ValidationDate { get; set; }
    }
}
