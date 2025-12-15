using System.ComponentModel.DataAnnotations;

namespace StudentTrackerCOMMON.Models
{
    public class Attendance
    {
        public int AttendanceID { get; set; }

        [Required]
        public int StudentID { get; set; }

        [Required]
        public int SessionID { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        public int CourseID { get; set; }
        public string? StudentName { get; set; }
        public string? Status { get; set; }
    }
}
