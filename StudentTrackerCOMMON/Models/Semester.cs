using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Models;

public class Semester
{
    public int SemesterID { get; set; }
    public int AcademicYearID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SemesterNb { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

