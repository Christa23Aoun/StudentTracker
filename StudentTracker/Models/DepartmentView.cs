namespace StudentTracker.Models
{
    public class DepartmentView
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public int CourseCount { get; set; }              // number of courses in department
        public string? CourseNames { get; set; }          // comma-separated list of course names
    }
}
