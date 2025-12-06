using System.Collections.Generic;

namespace StudentTracker.Models
{
    public class GenerateSessionsPageView
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public List<CourseSessionView> Sessions { get; set; } = new();
    }
}
