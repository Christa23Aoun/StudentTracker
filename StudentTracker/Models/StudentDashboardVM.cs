namespace StudentTracker.Models
{
    public class StudentDashboardVM
    {
        public int UserID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CurrentSemester { get; set; } = "-";
        public int ActiveCoursesCount { get; set; }
        public double GPA { get; set; }
        public double AttendancePercent { get; set; }

        public List<CourseCardVM> Courses { get; set; } = new();
        public List<NotificationVM> Notifications { get; set; } = new();

        public List<SeriesPointVM> GradeSeries { get; set; } = new();
        public List<SeriesPointVM> AttendanceSeries { get; set; } = new();

        public List<string> Departments { get; set; } = new();
        public List<string> Semesters { get; set; } = new();
        public string? SelectedDepartment { get; set; }
        public string? SelectedSemester { get; set; }
    }

    public class CourseCardVM
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public double AttendanceRate { get; set; }
        public double CurrentAverage { get; set; }
    }

    public class NotificationVM
    {
        public int NotificationID { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SeriesPointVM
    {
        public DateTime X { get; set; }
        public double Y { get; set; }
    }
    public class StudentCourseVM
    {
        public int StudentCourseID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }


}
