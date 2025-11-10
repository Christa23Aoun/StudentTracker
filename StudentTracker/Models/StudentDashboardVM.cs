using System;
using System.Collections.Generic;

namespace StudentTracker.Models.ViewModels
{
    public class StudentDashboardVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = string.Empty;
        public int ActiveCourseCount { get; set; }
        public double GPA { get; set; }
        public double AttendancePercent { get; set; }

        public List<CourseItemVM> MyCourses { get; set; } = new();
        public List<NotificationVM> Notifications { get; set; } = new();

        public List<GradePointVM> GradeProgress { get; set; } = new();
        public List<AttendancePointVM> AttendanceTrend { get; set; } = new();

        public List<string> Semesters { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public string? SelectedSemester { get; set; }
        public string? SelectedDepartment { get; set; }
    }

    public class CourseItemVM
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public double AttendanceRate { get; set; }
        public double CurrentAverage { get; set; }
    }

    public class NotificationVM
    {
        public int NotificationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info | warning | success | danger
        public bool IsRead { get; set; }
    }

    public class GradePointVM
    {
        public string Label { get; set; } = string.Empty; // e.g., "Test 1", "Midterm"
        public double Average { get; set; }
    }

    public class AttendancePointVM
    {
        public string WeekLabel { get; set; } = string.Empty; // e.g., "Wk1"
        public double Percent { get; set; }
    }
}