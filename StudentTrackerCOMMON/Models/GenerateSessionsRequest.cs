using System;

namespace StudentTrackerCOMMON.Models
{
    public class GenerateSessionsRequest
    {
        public int CourseID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string RepeatType { get; set; }
    }



}
