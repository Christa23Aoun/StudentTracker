namespace StudentTrackerCOMMON.DTOs.AdminDashboard
{
    // Root model returned by ONE API call (the unified dashboard model)
    public class AdminFullDashboardDto
    {
        // ✅ Summary cards (students, teachers, courses, departments)
        public DashboardSummaryDto Summary { get; set; } = new();

        // ✅ Academic overview
        public List<DepartmentCoursesDto> Departments { get; set; } = new();
        public List<CourseItemDto> Courses { get; set; } = new();

        // ✅ User overview
        public List<UserRoleDto> Users { get; set; } = new();

        // ✅ Grade validation section
        public List<PendingGradeDto> PendingGrades { get; set; } = new();
    }

    // --- Sub-DTOs for nested objects ---
    public class DashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public string CurrentAcademicYear { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
    }

    public class DepartmentCoursesDto
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int CourseCount { get; set; }
    }

    public class CourseItemDto
    {
        public int CourseID { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserRoleDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PendingGradeDto
    {
        public int TestGradeID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }
    }
}
