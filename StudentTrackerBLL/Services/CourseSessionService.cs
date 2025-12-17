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
        private readonly ISemesterRepository _semesters;
        private readonly IEnrollmentRepository _enrollments;
        private readonly INotificationService _notifications;

        public CourseSessionService(
            ICourseSessionRepository sessions,
            ICourseRepository courses,
            ISemesterRepository semesters,
            IEnrollmentRepository enrollments,
            INotificationService notifications)
        {
            _sessions = sessions;
            _courses = courses;
            _semesters = semesters;
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

            var semester = await _semesters.GetByIdAsync(course.SemesterID);
            if (semester == null)
                return false;

            var current = req.SessionDate.Date;
            var semesterEnd = semester.EndDate.Date;

            while (true)
            {
                var conflicts = await _sessions.GetTeacherConflictsAsync(
                    course.TeacherID,
                    current,
                    req.StartTime,
                    req.EndTime
                );

                if (conflicts.Any())
                    return false;

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

            var students = await _enrollments.GetStudentsByCourseAsync(req.CourseID);
            var message = $"New session(s) have been scheduled for {course.CourseName} starting {req.SessionDate:dd/MM/yyyy} from {req.StartTime:hh\\:mm} to {req.EndTime:hh\\:mm}.";

            foreach (var s in students)
            {
                await _notifications.NotifyStudentAsync(
                    s.UserID,
                    message,
                    "ADMIN",
                    $"/StudentDashboard/CourseDetails?courseId={req.CourseID}"
                );
            }

            return true;
        }

        public async Task<bool> DeleteSessionAsync(int sessionId)
        {
            return await _sessions.DeleteAsync(sessionId) > 0;
        }
    }
}
