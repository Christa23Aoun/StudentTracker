namespace StudentTrackerCOMMON.DTOs.AdminDashboard
{
    public class AdminDashboardDto
    {
        public AdminDashboardSummaryDto Summary { get; set; } = new();
        public List<DepartmentDashboardDto> Departments { get; set; } = new();
        public List<CourseDashboardDto> Courses { get; set; } = new();
        public List<UserDashboardDto> Users { get; set; } = new();
        public List<AdminPendingGradeDto> PendingGrades { get; set; } = new();
    }

    public class AdminDashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public string CurrentAcademicYear { get; set; } = "";
        public string CurrentSemester { get; set; } = "";
    }

    public class DepartmentDashboardDto
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = "";
        public int CourseCount { get; set; }
    }

    public class CourseDashboardDto
    {
        public int CourseID { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class UserDashboardDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
        public bool IsActive { get; set; }
    }

    // ✅ renamed to avoid ambiguity
    public class AdminPendingGradeDto
    {
        public int TestGradeID { get; set; }
        public int TestID { get; set; }
        public int StudentID { get; set; }
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }
    }
}
