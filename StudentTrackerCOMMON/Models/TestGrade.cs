using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerCOMMON.Models
{
    public class TestGrade
    {
        public int TestGradeID { get; set; }
        public int TestID { get; set; }
        public int StudentID { get; set; }
        public decimal Score { get; set; }
        public bool IsValidated { get; set; }
        public DateTime? ValidationDate { get; set; }
    }
}

