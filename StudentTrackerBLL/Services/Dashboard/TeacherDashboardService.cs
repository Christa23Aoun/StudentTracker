using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class TeacherDashboardService
    {
        private readonly ICourseRepository _courses;
        private readonly IAttendanceRepository _attendance;
        private readonly ITestGradeRepository _grades;

        public TeacherDashboardService(
            ICourseRepository courses,
            IAttendanceRepository attendance,
            ITestGradeRepository grades)
        {
            _courses = courses;
            _attendance = attendance;
            _grades = grades;
        }

        // 🧩 Main method: gather all info for one teacher
        public async Task<TeacherDashboardDto> GetDashboardAsync(int teacherId)
        {
            // 1️⃣ Get courses taught by the teacher
            var myCourses = await _courses.GetByTeacherIdAsync(teacherId);

            // 2️⃣ Build DTO
            var dto = new TeacherDashboardDto
            {
                CourseCount = myCourses?.Count ?? 0,
                StudentCount = 0,
                AverageGrade = 0,
                AttendanceRate = 0
            };

            var courseRows = new List<TeacherCourseRowDto>();

            // 3️⃣ For each course, fetch related info
            if (myCourses != null)
            {
                foreach (var c in myCourses)
                {
                    var students = await _courses.GetEnrolledStudentsAsync(c.CourseID);
                    var attendanceRate = await _attendance.GetAverageAttendanceByCourseAsync(c.CourseID);
                    var avgGrade = await _grades.GetAverageGradeByCourseAsync(c.CourseID);

                    dto.StudentCount += students.Count;
                    courseRows.Add(new TeacherCourseRowDto
                    {
                        CourseID = c.CourseID,
                        CourseName = c.CourseName,
                        SemesterName = $"Semester {c.SemesterID}",
                        StudentCount = students.Count,
                        AttendanceRate = attendanceRate,
                        AverageGrade = avgGrade
                    });
                }

                dto.Courses = courseRows;

                // 4️⃣ Global averages
                if (courseRows.Count > 0)
                {
                    dto.AverageGrade = Math.Round(courseRows.Average(x => x.AverageGrade), 1);
                    dto.AttendanceRate = Math.Round(courseRows.Average(x => x.AttendanceRate), 1);
                }
            }

            // 5️⃣ Temporary Recent Activities (mock until logs are linked)
            dto.RecentActivities = new List<RecentActivityDto>
            {
                new() { Timestamp = DateTime.Now.AddHours(-2), Type = "Test", Description = "Added midterm for Algorithms" },
                new() { Timestamp = DateTime.Now.AddHours(-4), Type = "Grade", Description = "Submitted grades for Data Structures" },
                new() { Timestamp = DateTime.Now.AddHours(-6), Type = "Attendance", Description = "Marked attendance for Programming II" }
            };

            // 6️⃣ Sample chart data
            dto.AttendanceVsPerformance = new List<SeriesPointDto>
            {
                new() { Label = "Algorithms", Y = 88 },
                new() { Label = "Data Structures", Y = 75 },
                new() { Label = "Programming II", Y = 91 }
            };

            return dto;
        }
    }
}
