using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class TeacherDashboardService : ITeacherDashboardService
    {
        private readonly ICourseRepository _courses;
        private readonly ICourseSessionRepository _sessions;

        public TeacherDashboardService(
            ICourseRepository courses,
            ICourseSessionRepository sessions)
        {
            _courses = courses;
            _sessions = sessions;
        }

        public async Task<TeacherDashboardDto> GetDashboardAsync(int userId)
        {
            var courseStats = (await _courses.GetCourseStatsByTeacherAsync(userId)).ToList();

            var dto = new TeacherDashboardDto
            {
                CourseCount = courseStats.Count,
                StudentCount = courseStats.Sum(c => c.StudentCount),
                Courses = courseStats
            };

            if (courseStats.Count > 0)
            {
                dto.AverageGrade = Math.Round(courseStats.Average(c => c.AverageGrade), 1);
                dto.AttendanceRate = Math.Round(courseStats.Average(c => c.AttendanceRate), 1);
            }

            return dto;
        }

        public async Task<IEnumerable<TeacherScheduleItemDto>> GetTeacherWeeklyScheduleAsync(
    int teacherId,
    DateTime weekStart,
    DateTime weekEnd)
        {
            return await _sessions.GetTeacherWeeklyScheduleAsync(
                teacherId,
                weekStart,
                weekEnd
            );
        }


    }
}
