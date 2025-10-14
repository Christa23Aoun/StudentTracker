using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models;

public class SemesterView
{
    public int SemesterID { get; set; }

    [Required]
    [Display(Name = "Academic Year")]
    public int AcademicYearID { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 8)]
    [Display(Name = "Semester Number")]
    public int SemesterNb { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    // For dropdown rendering
    public IEnumerable<AcademicYearOption> AcademicYears { get; set; } = new List<AcademicYearOption>();
}

public class AcademicYearOption
{
    public int AcademicYearID { get; set; }
    public string Name { get; set; } = string.Empty;
}
