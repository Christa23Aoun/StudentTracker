using System;
using System.Collections.Generic;

namespace StudentTrackerCOMMON.DTOs
{
    public class StudentDashboardDTO
    {
        public string StudentName { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
        public int ActiveCourseCount { get; set; }
        public double GPA { get; set; }
        public double AttendancePercent { get; set; }
        public List<CourseItemDTO> MyCourses { get; set; } = new();
        public List<NotificationDTO> Notifications { get; set; } = new();
        public List<GradePointDTO> GradeProgress { get; set; } = new();
        public List<AttendancePointDTO> AttendanceTrend { get; set; } = new();
    }

    public class CourseItemDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public double AttendanceRate { get; set; }
        public double CurrentAverage { get; set; }
    }

    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class GradePointDTO
    {
        public string Label { get; set; } = string.Empty;
        public double Average { get; set; }
    }

    public class AttendancePointDTO
    {
        public string WeekLabel { get; set; } = string.Empty;
        public double Percent { get; set; }
    }
}
