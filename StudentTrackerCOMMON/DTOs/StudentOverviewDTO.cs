namespace StudentTrackerCOMMON.DTOs
{
    public class StudentOverviewDTO
    {
        public string StudentName { get; set; } = "";
        public string CurrentSemester { get; set; } = "";
        public int ActiveCourseCount { get; set; }
        public double GPA { get; set; }
        public double AttendancePercent { get; set; }
    }
}
