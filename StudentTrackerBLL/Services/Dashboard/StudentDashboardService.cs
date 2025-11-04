using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentTrackerCOMMON.DTOs;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class StudentDashboardService
    {
        public StudentDashboardService() { }

        // This method now runs standalone – no DAL calls
        public async Task<StudentDashboardDTO> GetStudentDashboardAsync(int studentId)
        {
            await Task.Delay(100); // simulate async call

            // Return static mock data so the API and MVC can load successfully
            return new StudentDashboardDTO
            {
                StudentName = "Lynn El-Haly",
                CurrentSemester = "Fall 2025",
                ActiveCourseCount = 4,
                GPA = 3.42,
                AttendancePercent = 92.5,
                MyCourses = new List<CourseItemDTO>
                {
                    new() { CourseId = 1, CourseName = "Operating Systems", TeacherName = "Dr. Tannous", Department = "CS", AttendanceRate = 95, CurrentAverage = 88 },
                    new() { CourseId = 2, CourseName = "Data Structures", TeacherName = "Dr. Gerges", Department = "CS", AttendanceRate = 90, CurrentAverage = 91 },
                    new() { CourseId = 3, CourseName = "Discrete Math", TeacherName = "Dr. Hachem", Department = "Math", AttendanceRate = 89, CurrentAverage = 84 },
                    new() { CourseId = 4, CourseName = "Physics I", TeacherName = "Dr. Salem", Department = "Physics", AttendanceRate = 96, CurrentAverage = 86 }
                },
                Notifications = new List<NotificationDTO>
                {
                    new() { NotificationId = 101, Title = "New grade posted", Message = "OS Midterm grade is available.", Type = "success", CreatedAt = DateTime.UtcNow, IsRead = false },
                    new() { NotificationId = 102, Title = "Attendance Warning", Message = "Data Structures attendance below 75%.", Type = "warning", CreatedAt = DateTime.UtcNow.AddDays(-2), IsRead = false },
                    new() { NotificationId = 103, Title = "Course update", Message = "Discrete Math room changed to B302.", Type = "info", CreatedAt = DateTime.UtcNow.AddDays(-7), IsRead = true }
                },
                GradeProgress = new List<GradePointDTO>
                {
                    new() { Label = "Quiz 1", Average = 78 },
                    new() { Label = "Quiz 2", Average = 82 },
                    new() { Label = "Midterm", Average = 86 },
                    new() { Label = "Project", Average = 90 }
                },
                AttendanceTrend = new List<AttendancePointDTO>
                {
                    new() { WeekLabel = "Wk1", Percent = 100 },
                    new() { WeekLabel = "Wk2", Percent = 95 },
                    new() { WeekLabel = "Wk3", Percent = 88 },
                    new() { WeekLabel = "Wk4", Percent = 92 },
                    new() { WeekLabel = "Wk5", Percent = 94 }
                }
            };
        }
    }
}
