using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.DTOs.TeacherDashboard
{
    public class SeriesPointDto
    {
        public string Label { get; set; } = string.Empty; // "Week 1", "Algorithms I", etc.
        public decimal X { get; set; }                    // optional numeric x (can be 0 if unused)
        public decimal Y { get; set; }                    // the value to plot (e.g., 85.5)
    }
}
