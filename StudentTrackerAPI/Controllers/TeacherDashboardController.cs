using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/TeacherDashboard")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly ICourseRepository _courses;
        private readonly IAttendanceRepository _attendance;
        private readonly ITestGradeRepository _grades;

        public TeacherDashboardController(
            ICourseRepository courses,
            IAttendanceRepository attendance,
            ITestGradeRepository grades)
        {
            _courses = courses;
            _attendance = attendance;
            _grades = grades;
        }

       
        [HttpGet("Teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherDashboard(int teacherId)
        {
          

            var courses = await _courses.GetByTeacherIdAsync(teacherId);
            int totalCourses = courses.Count();

         
            int totalStudents = 0;
            foreach (var course in courses)
            {
                var students = await _courses.GetEnrolledStudentsAsync(course.CourseID);
                totalStudents += students.Count;
            }

            decimal avgGrade = 85;       
            decimal attendanceRate = 90; 

            var activities = new List<RecentActivityDto>
            {
                new() { Timestamp = DateTime.UtcNow.AddDays(-1), Description = "Recorded attendance for Algorithms class" },
                new() { Timestamp = DateTime.UtcNow.AddDays(-2), Description = "Created new test: Midterm Exam" },
                new() { Timestamp = DateTime.UtcNow.AddDays(-3), Description = "Updated grades for Data Structures" }
            };

            var dto = new TeacherDashboardDto
            {
                CourseCount = totalCourses,
                StudentCount = totalStudents,
                AverageGrade = avgGrade,
                AttendanceRate = attendanceRate,
                RecentActivities = activities
            };

            return Ok(dto);
        }
    }
}
