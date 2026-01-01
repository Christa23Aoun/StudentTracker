using System;

namespace StudentTracker.Models
{
    public class TeacherScheduleItemView
    {
        public int CourseID { get; set; }

        public string CourseName { get; set; } = "";
        public string CourseCode { get; set; } = "";

        public DateTime SessionDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
