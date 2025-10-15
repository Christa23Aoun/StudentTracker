using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Models;

public class Course
{
    public int CourseID { get; set; }
    public string? CourseCode { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int DepartmentID { get; set; }
    public int TeacherID { get; set; }
    public int SemesterID { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
