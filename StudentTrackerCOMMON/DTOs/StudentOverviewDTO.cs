namespace StudentTrackerCOMMON.DTOs
{
    public class StudentOverviewDTO
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
        public int ActiveCourseCount { get; set; }
        public double GPA { get; set; }
        public double AttendancePercent { get; set; }
        public double AverageGrade { get; set; }
    }
}
