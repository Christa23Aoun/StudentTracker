namespace StudentTrackerCOMMON.Models
{
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public DateTime AttendanceDate { get; set; }

        public bool IsPresent { get; set; }
        public bool IsValidated { get; set; }

        public string Status { get; set; }    // Present / Late / Absent
        public string StudentName { get; set; } = "";
        public string CourseName { get; set; } = "";
    }
}
