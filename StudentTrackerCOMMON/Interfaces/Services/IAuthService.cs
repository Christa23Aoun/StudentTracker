using StudentTrackerCOMMON.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Interfaces.Services;

public interface IAuthService
{
    Task<int> RegisterAsync(RegisterDto dto);   // returns new UserID
    Task<bool> LoginAsync(LoginDto dto);        // true/false for now
}
