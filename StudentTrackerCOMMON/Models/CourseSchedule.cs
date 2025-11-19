using System;

namespace StudentTrackerCOMMON.Models
{
    public class CourseSchedule
    {
        public int ScheduleID { get; set; }
        public int CourseID { get; set; }
        public byte DayOfWeek { get; set; }  // 1 = Monday ... 7 = Sunday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
