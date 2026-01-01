using StudentTrackerCOMMON.DTOs;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerCOMMON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<bool> AddSessionAsync(CourseSession session)
        {
            var result = await _sessions.CreateAsync(session);
            return result > 0;
        }

        public async Task<bool> GenerateSessionsAsync(GenerateSessionsRequest req)
        {
            var course = await _courses.GetByIdAsync(req.CourseID);
            if (course == null)
                return false;

            var students = await _enrollments.GetStudentsByCourseAsync(req.CourseID);

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

                    foreach (var s in students)
                    {
                        await _notifications.CreateAsync(new Notification
                        {
                            UserID = s.UserID,
                            Message = $"New session(s) have been scheduled for {course.CourseName} starting {current:dd/MM/yyyy} from {req.StartTime:hh\\:mm} to {req.EndTime:hh\\:mm}.",
                            Type = "info",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        });
                    }
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
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null)
                return false;

            var course = await _courses.GetByIdAsync(session.CourseID);
            if (course == null)
                return false;

            var students = await _enrollments.GetStudentsByCourseAsync(course.CourseID);

            var deleted = await _sessions.DeleteAsync(sessionId) > 0;

            if (deleted)
            {
                foreach (var s in students)
                {
                    await _notifications.CreateAsync(new Notification
                    {
                        UserID = s.UserID,
                        Message = $"A session for {course.CourseName} on {session.SessionDate:dd/MM/yyyy} from {session.StartTime:hh\\:mm} to {session.EndTime:hh\\:mm} has been cancelled.",
                        Type = "warning",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }
            }

            return deleted;
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
