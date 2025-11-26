using System;
using System.Collections.Generic;

namespace StudentTracker.Models
{
    public class StudentCourseDetailsVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public int CreditHours { get; set; }
        public double AttendanceRate { get; set; }
        public double CurrentAverage { get; set; }

        public List<StudentTestGradeItemVM> Tests { get; set; } = new();
        public List<StudentAttendanceItemVM> Attendance { get; set; } = new();
    }


    public class StudentTestGradeItemVM
    {
        public string TestName { get; set; } = "";
        public DateTime TestDate { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }

        public double StudentScore { get; set; }
        public int IsValidated { get; set; }
    }


    public class StudentAttendanceItemVM
    {
        public DateTime AttendanceDate { get; set; }
        public bool IsPresent { get; set; }
    }

}
