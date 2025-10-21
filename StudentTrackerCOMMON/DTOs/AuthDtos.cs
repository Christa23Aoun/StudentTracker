using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs;

public record RegisterDto(string FullName, string Email, string Password, int RoleID);
public record LoginDto(string Email, string Password);

