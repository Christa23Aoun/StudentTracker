namespace StudentTrackerCOMMON.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
        public string? CourseNames { get; set; }  
    }
}
