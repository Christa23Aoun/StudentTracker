namespace StudentTracker.Models
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardSummary Summary { get; set; } = new();

        // ✅ Already existing lists
        public List<DepartmentDashboardView>? Departments { get; set; }
        public List<CourseDashboardView>? Courses { get; set; }
        public List<UserDashboardView>? Users { get; set; }

        // ✅ New: Pending Grades and Attendance count
        public List<AdminPendingGradeView>? PendingGrades { get; set; } = new();
        public int AttendanceCount { get; set; } = 0;
    }

    public class AdminDashboardSummary
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveCourses { get; set; }
        public int Departments { get; set; }
        public string CurrentAcademicYear { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
    }

    public class DepartmentDashboardView
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int CourseCount { get; set; }
    }

    public class CourseDashboardView
    {
        public int CourseID { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserDashboardView
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ✅ New: Pending Grades View (mini DTO)
    public class AdminPendingGradeView
    {
        public int TestID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double? Grade { get; set; }
        public bool IsValidated { get; set; }
    }
}
