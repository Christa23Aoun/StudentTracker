using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using System;
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

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetById(int sessionId)
        {
            var item = await _sessions.GetByIdAsync(sessionId);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseSession session)
        {
            var id = await _sessions.CreateAsync(session);
            return Ok(new { SessionID = id });
        }

        [HttpPut("{sessionId}")]
        public async Task<IActionResult> Update(int sessionId, [FromBody] CourseSession session)
        {
            session.SessionID = sessionId;
            await _sessions.UpdateAsync(session);
            return Ok();
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> Delete(int sessionId)
        {
            await _sessions.DeleteAsync(sessionId);
            return Ok();
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

            DateTime current = req.SessionDate.Date;
            DateTime semesterEnd = semester.EndDate.Date;

            if (req.RepeatType == "OneTime")
            {
                await _sessions.CreateAsync(new CourseSession
                {
                    CourseID = req.CourseID,
                    SessionDate = current,
                    StartTime = req.StartTime,
                    EndTime = req.EndTime,
                    IsCancelled = false,
                    RepeatType = "OneTime"
                });


                return Ok();
            }

            DateTime end = req.RepeatType == "HalfSemester"
                ? current.AddDays(7 * 7)
                : semesterEnd;

            if (end > semesterEnd)
                end = semesterEnd;

            while (current <= end)
            {
                await _sessions.CreateAsync(new CourseSession
                {
                    CourseID = req.CourseID,
                    SessionDate = current,
                    StartTime = req.StartTime,
                    EndTime = req.EndTime,
                    IsCancelled = false,
                    RepeatType = "OneTime"
                });


                current = current.AddDays(7);
            }

            return Ok();
        }

    }
}
