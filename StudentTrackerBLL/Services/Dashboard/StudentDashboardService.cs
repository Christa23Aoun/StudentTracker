using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly IStudentDashboardRepository _repo;

        public StudentDashboardService(IStudentDashboardRepository repo)
        {
            _repo = repo;
        }

        public async Task<StudentDashboardDTO> GetStudentDashboardAsync(int studentId)
        {
            var dto = new StudentDashboardDTO();

            // 1. Overview
            var overview = await _repo.GetOverviewAsync(studentId);
            if (overview != null)
            {
                dto.StudentName = overview.StudentName;
                dto.CurrentSemester = overview.CurrentSemester;
                dto.ActiveCourseCount = overview.ActiveCourseCount;
                dto.GPA = overview.GPA;
                dto.AttendancePercent = overview.AttendancePercent;
            }

            // 2. Courses
            var courses = await _repo.GetCoursesAsync(studentId);
            dto.MyCourses = courses.ToList();

            // 3. Notifications
            var notifs = await _repo.GetNotificationsAsync(studentId);
            dto.Notifications = notifs.ToList();

            // 4. Grade Progress
            var grades = await _repo.GetGradeProgressAsync(studentId);
            dto.GradeProgress = grades.ToList();

            // 5. Attendance Trend
            var attendance = await _repo.GetAttendanceTrendAsync(studentId);
            dto.AttendanceTrend = attendance.ToList();

            return dto;
        }
    }
}
