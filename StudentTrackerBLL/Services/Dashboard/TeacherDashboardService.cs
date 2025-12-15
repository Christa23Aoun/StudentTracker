using StudentTrackerCOMMON.DTOs.TeacherDashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerBLL.Services.Dashboard
{
    public class TeacherDashboardService
    {
        private readonly ICourseRepository _courses;
        private readonly ITestGradeRepository _grades;

        public TeacherDashboardService(
            ICourseRepository courses,
            ITestGradeRepository grades)
        {
            _courses = courses;
            _grades = grades;
        }

        public async Task<TeacherDashboardDto> GetDashboardAsync(int teacherId)
        {
            var myCourses = await _courses.GetByTeacherIdAsync(teacherId);

            var dto = new TeacherDashboardDto
            {
                CourseCount = myCourses?.Count ?? 0,
                StudentCount = 0,
                AverageGrade = 0,
                AttendanceRate = 0
            };

            var courseRows = new List<TeacherCourseRowDto>();

            if (myCourses != null)
            {
                foreach (var c in myCourses)
                {
                    var students = await _courses.GetEnrolledStudentsAsync(c.CourseID);
                    var avgGrade = await _grades.GetAverageGradeByCourseAsync(c.CourseID);

                    dto.StudentCount += students.Count;

                    courseRows.Add(new TeacherCourseRowDto
                    {
                        CourseID = c.CourseID,
                        CourseName = c.CourseName,
                        SemesterName = $"Semester {c.SemesterID}",
                        StudentCount = students.Count,
                        AttendanceRate = 0,
                        AverageGrade = avgGrade
                    });
                }

                dto.Courses = courseRows;

                if (courseRows.Count > 0)
                {
                    dto.AverageGrade = Math.Round(courseRows.Average(x => x.AverageGrade), 1);
                    dto.AttendanceRate = 0;
                }
            }

            dto.RecentActivities = new List<RecentActivityDto>
            {
                new() { Timestamp = DateTime.Now.AddHours(-2), Type = "Test", Description = "Added midterm for Algorithms" },
                new() { Timestamp = DateTime.Now.AddHours(-4), Type = "Grade", Description = "Submitted grades for Data Structures" },
                new() { Timestamp = DateTime.Now.AddHours(-6), Type = "Attendance", Description = "Marked attendance for Programming II" }
            };

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
