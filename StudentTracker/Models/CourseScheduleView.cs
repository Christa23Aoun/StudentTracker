using System;

namespace StudentTracker.Models
{
    public class CourseScheduleView
    {
        public int ScheduleID { get; set; }
        public int CourseID { get; set; }
        public byte DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
