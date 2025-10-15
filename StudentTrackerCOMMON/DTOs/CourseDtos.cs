using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs;

public record CourseCreateDto(
    string? CourseCode,
    string CourseName,
    int CreditHours,
    int DepartmentID,
    int TeacherID,
    int SemesterID,
    bool IsActive
);

public record CourseUpdateDto(
    int CourseID,
    string? CourseCode,
    string CourseName,
    int CreditHours,
    int DepartmentID,
    int TeacherID,
    int SemesterID,
    bool IsActive
);

