using System;

namespace StudentTracker.Models
{
    public class CourseSessionView
    {
        public int SessionID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCancelled { get; set; }
        public string RepeatType { get; set; } = "OneTime";
    }
}
