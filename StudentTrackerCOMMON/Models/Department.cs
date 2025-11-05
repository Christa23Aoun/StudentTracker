namespace StudentTrackerCOMMON.Models;

public class Department
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
//batata
