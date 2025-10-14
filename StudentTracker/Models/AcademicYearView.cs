using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models;

public class AcademicYearView
{
    public int AcademicYearID { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
