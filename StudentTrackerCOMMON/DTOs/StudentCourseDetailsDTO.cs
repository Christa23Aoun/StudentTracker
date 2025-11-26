using System;
using System.Collections.Generic;

namespace StudentTrackerCOMMON.DTOs
{
    public class StudentCourseDetailsDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public int CreditHours { get; set; }
        public double AttendanceRate { get; set; }
        public double CurrentAverage { get; set; }

        public List<StudentTestGradeItemDTO> Tests { get; set; } = new();
        public List<StudentAttendanceItemDTO> Attendance { get; set; } = new();
    }

    public class StudentTestGradeItemDTO
    {
        public string TestName { get; set; } = "";
        public DateTime TestDate { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }

        // MUST MATCH SP COLUMNS EXACTLY
        public double StudentScore { get; set; }   // instead of Score
        public int IsValidated { get; set; }       // instead of Status
    }

    public class StudentAttendanceItemDTO
    {
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }

}
