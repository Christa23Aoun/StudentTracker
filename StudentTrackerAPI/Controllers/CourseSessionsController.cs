using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseSessionsController : ControllerBase
    {
        private readonly ICourseSessionRepository _sessions;
        private readonly ICourseRepository _courses;
        private readonly ISemesterRepository _semesters;

        public CourseSessionsController(
            ICourseSessionRepository sessions,
            ICourseRepository courses,
            ISemesterRepository semesters)
        {
            _sessions = sessions;
            _courses = courses;
            _semesters = semesters;
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var list = await _sessions.GetByCourseAsync(courseId);
            return Ok(list);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSessions([FromBody] GenerateSessionsRequest req)
        {
            var course = await _courses.GetByIdAsync(req.CourseID);
            if (course == null)
                return NotFound();

            var semester = await _semesters.GetByIdAsync(course.SemesterID);
            if (semester == null)
                return NotFound();

            var teacherId = course.TeacherID;
            var current = req.SessionDate.Date;
            var semesterEnd = semester.EndDate.Date;

            while (true)
            {
                var conflicts = await _sessions.GetTeacherConflictsAsync(
                    teacherId,
                    current,
                    req.StartTime,
                    req.EndTime
                );

                var conflict = conflicts.FirstOrDefault();
                if (conflict != null)
                {
                    return BadRequest(
                        $"Teacher {conflict.TeacherName} already has a session at this time for course {conflict.CourseName}."
                    );
                }

                await _sessions.CreateAsync(new CourseSession
                {
                    CourseID = req.CourseID,
                    SessionDate = current,
                    StartTime = req.StartTime,
                    EndTime = req.EndTime,
                    IsCancelled = false,
                    RepeatType = req.RepeatType
                });

                if (req.RepeatType == "None")
                    break;

                current = req.RepeatType switch
                {
                    "Daily" => current.AddDays(1),
                    "Weekly" => current.AddDays(7),
                    "Monthly" => current.AddMonths(1),
                    _ => semesterEnd.AddDays(1)
                };

                if (current > semesterEnd)
                    break;
            }

            return Ok();
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> Delete(int sessionId)
        {
            await _sessions.DeleteAsync(sessionId);
            return Ok();
        }
    }
}
