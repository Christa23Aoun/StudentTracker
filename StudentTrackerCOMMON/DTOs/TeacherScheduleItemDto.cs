using System;

namespace StudentTrackerCOMMON.DTOs
{
    public class TeacherScheduleItemDto
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;

        public DateTime SessionDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Room { get; set; } = string.Empty;
    }
}
