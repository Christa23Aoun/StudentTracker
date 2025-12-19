using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StudentTrackerBLL.Services
{
    public class CourseSessionService : ICourseSessionService
    {
        private readonly ICourseSessionRepository _sessions;
        private readonly ICourseRepository _courses;
        private readonly IEnrollmentRepository _enrollments;
        private readonly INotificationService _notifications;

        public CourseSessionService(
            ICourseSessionRepository sessions,
            ICourseRepository courses,
            IEnrollmentRepository enrollments,
            INotificationService notifications)
        {
            _sessions = sessions;
            _courses = courses;
            _enrollments = enrollments;
            _notifications = notifications;
        }

        public async Task<IEnumerable<CourseSession>> GetByCourseAsync(int courseId)
        {
            return await _sessions.GetByCourseAsync(courseId);
        }

        public async Task<bool> GenerateSessionsAsync(GenerateSessionsRequest req)
        {
            var course = await _courses.GetByIdAsync(req.CourseID);
            if (course == null)
                return false;

            var current = req.StartDate.Date;
            var endDate = req.EndDate.Date;

            while (true)
            {
                var conflicts = await _sessions.GetTeacherConflictsAsync(
                    course.TeacherID,
                    current,
                    req.StartTime,
                    req.EndTime
                );

                if (!conflicts.Any())
                {
                    await _sessions.CreateAsync(new CourseSession
                    {
                        CourseID = req.CourseID,
                        SessionDate = current,
                        StartTime = req.StartTime,
                        EndTime = req.EndTime,
                        IsCancelled = false,
                        RepeatType = req.RepeatType
                    });
                }

                if (req.RepeatType == "None")
                    break;

                current = req.RepeatType switch
                {
                    "Daily" => current.AddDays(1),
                    "Weekly" => current.AddDays(7),
                    _ => endDate.AddDays(1)
                };

                if (current > endDate)
                    break;
            }

            return true;
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            return await _sessions.DeleteAsync(sessionId) > 0;
        }
    }
}
