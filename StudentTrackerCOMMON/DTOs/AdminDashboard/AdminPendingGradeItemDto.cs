using System;

namespace StudentTrackerCOMMON.DTOs.AdminDashboard
{
    public class AdminPendingGradeItemDto
    {
        public int TestGradeID { get; set; }

        public int TestID { get; set; }
        public string TestName { get; set; } = string.Empty;

        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
