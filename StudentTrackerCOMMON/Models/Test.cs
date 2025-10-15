using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Models
{
    public class Test
    {
        public int TestID { get; set; }
        public int CourseID { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        public decimal Weight { get; set; }
        public decimal MaxScore { get; set; }
    }
}
