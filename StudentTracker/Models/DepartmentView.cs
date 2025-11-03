namespace StudentTracker.Models
{
    public class DepartmentView
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
