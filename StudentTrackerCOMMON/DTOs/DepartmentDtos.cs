using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs;

public record DepartmentCreateDto(string DepartmentName);
public record DepartmentUpdateDto(int DepartmentID, string DepartmentName);

