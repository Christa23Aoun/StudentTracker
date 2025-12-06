using System;

namespace StudentTrackerCOMMON.Models
{
    public class CourseSession
    {
        public int SessionID { get; set; }
        public int CourseID { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsCancelled { get; set; }
        public string RepeatType { get; set; } = "OneTime";
    }
}
