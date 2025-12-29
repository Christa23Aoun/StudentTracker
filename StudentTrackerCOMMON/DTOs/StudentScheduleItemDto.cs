using System;

namespace StudentTrackerCOMMON.DTOs
{
    public class StudentScheduleItemDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Room { get; set; } = string.Empty;
    }
}
