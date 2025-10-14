using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs;

public record AcademicYearCreateDto(string Name, DateTime StartDate, DateTime EndDate);
public record AcademicYearUpdateDto(int AcademicYearID, string Name, DateTime StartDate, DateTime EndDate);

