using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs;

public record SemesterCreateDto(
    int AcademicYearID,
    string Name,
    int SemesterNb,
    DateTime StartDate,
    DateTime EndDate
);

public record SemesterUpdateDto(
    int SemesterID,
    int AcademicYearID,
    string Name,
    int SemesterNb,
    DateTime StartDate,
    DateTime EndDate
);
