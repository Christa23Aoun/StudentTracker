using System;

namespace StudentTracker.Models
{
    public class StudentScheduleItemView
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = "";
        public string CourseCode { get; set; } = "";

        public int DayNumber { get; set; }

        public DayOfWeek Day =>
            DayNumber == 1 ? DayOfWeek.Sunday :
            (DayOfWeek)(DayNumber - 1);

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Room { get; set; } = "";
    }
}
