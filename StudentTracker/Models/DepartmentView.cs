namespace StudentTracker.Models
{
    public class DepartmentView
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public int CourseCount { get; set; }              
        public string? CourseNames { get; set; }
        public List<CourseView>? Courses { get; set; } = new();
    }
}
