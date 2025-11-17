using System.Collections.Generic;

namespace StudentTracker.Models
{
    public class EnrollmentViewModel
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<DepartmentCoursesView> Departments { get; set; } = new();
        public List<int> SelectedCourses { get; set; } = new();
        public List<int> EnrolledCourseIds { get; set; } = new();
        public List<int> DroppedCourseIds { get; set; } = new();
        public string? ReturnRole { get; set; }
        public string? ReturnStatus { get; set; }
    }

    public class DepartmentCoursesView
    {
        public string DepartmentName { get; set; } = string.Empty;
        public List<SimpleCourseView> Courses { get; set; } = new();
    }

    public class SimpleCourseView
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }
}
