using System;

namespace StudentTrackerCOMMON.Models
{
    public class TeacherSessionConflict
    {
        public string TeacherName { get; set; }
        public string CourseName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
