using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _courses;
        private readonly ITestGradeRepository _grades;

        public CoursesController(
            ICourseRepository courses,
            ITestGradeRepository grades)
        {
            _courses = courses;
            _grades = grades;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _courses.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _courses.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Course course)
        {
            var id = await _courses.CreateAsync(course);
            return Ok(new { CourseID = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Course course)
        {
            course.CourseID = id;
            await _courses.UpdateAsync(course);
            return Ok(new { message = "Course updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courses.DeleteAsync(id);
            return Ok(new { message = "Course deleted successfully" });
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var affected = await _courses.DeactivateAsync(id);
            if (affected <= 0)
                return NotFound(new { message = "Course not found or already deactivated" });

            return Ok(new { message = "Course deactivated successfully" });
        }

        [HttpGet("byTeacher/{teacherId}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
        {
            var rawCourses = await _courses.GetByTeacherIdAsync(teacherId);
            var result = new List<object>();

            foreach (var c in rawCourses)
            {
                var students = await _courses.GetEnrolledStudentsAsync(c.CourseID);
                var studentCount = students.Count;

                double avgGrade = 0;

                try { avgGrade = (double)(await _grades.GetAverageGradeByCourseAsync(c.CourseID)); }
                catch { }

                result.Add(new
                {
                    c.CourseID,
                    c.CourseCode,
                    c.CourseName,
                    c.DepartmentID,
                    c.SemesterID,
                    c.TeacherID,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    StudentCount = studentCount,
                    AverageGrade = avgGrade,
                    AttendanceRate = 0
                });
            }

            return Ok(result);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var courseItem = await _courses.GetByIdAsync(id);
            if (courseItem == null)
                return NotFound();

            var students = await _courses.GetEnrolledStudentsAsync(courseItem.CourseID);
            var studentCount = students.Count;

            double avgGrade = 0;

            try { avgGrade = (double)(await _grades.GetAverageGradeByCourseAsync(courseItem.CourseID)); }
            catch { }

            var dto = new
            {
                courseItem.CourseID,
                courseItem.CourseCode,
                courseItem.CourseName,
                courseItem.DepartmentID,
                courseItem.SemesterID,
                courseItem.TeacherID,
                courseItem.IsActive,
                courseItem.DepartmentName,
                courseItem.SemesterName,
                StudentCount = studentCount,
                AverageGrade = avgGrade,
                AttendanceRate = 0
            };

            return Ok(dto);
        }
    }
}
