namespace StudentTracker.Models
{
    public class AdminDashboardViewModel
    {
        public DashboardSummary? Summary { get; set; }
        public List<DepartmentInfo>? Departments { get; set; }
        public List<CourseInfo>? Courses { get; set; }
        public List<UserInfo>? Users { get; set; }
        public List<PendingGrade>? PendingGrades { get; set; }
    }

    public class DashboardSummary
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public string? CurrentAcademicYear { get; set; }
        public string? CurrentSemester { get; set; }
    }

    public class DepartmentInfo
    {
        public int DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public int CourseCount { get; set; }
    }

    public class CourseInfo
    {
        public int CourseID { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? DepartmentName { get; set; }
        public string? TeacherName { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserInfo
    {
        public int UserID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }

    public class PendingGrade
    {
        public int TestGradeID { get; set; }
        public int StudentID { get; set; }
        public string? StudentName { get; set; }
        public string? CourseName { get; set; }
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }
    }
}
