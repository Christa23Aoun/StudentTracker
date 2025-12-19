using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class TeacherDashboardService
    {
        private readonly ICourseRepository _courses;

        public TeacherDashboardService(ICourseRepository courses)
        {
            _courses = courses;
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
    }
}
