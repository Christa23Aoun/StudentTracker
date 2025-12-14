using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Infrastructure;

namespace StudentTrackerDAL.Repositories
{
    public class CourseSessionRepository : ICourseSessionRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public CourseSessionRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<int> CreateAsync(CourseSession session)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.CourseSessions_Create",
                new
                {
                    session.CourseID,
                    session.SessionDate,
                    session.StartTime,
                    session.EndTime,
                    session.IsCancelled
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<CourseSession?> GetByIdAsync(int sessionId)
        {
            using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<CourseSession>(
                "dbo.CourseSessions_GetById",
                new { SessionID = sessionId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<CourseSession>> GetByCourseAsync(int courseId)
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<CourseSession>(
                "dbo.CourseSessions_GetByCourse",
                new { CourseID = courseId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<TeacherSessionConflict>> GetTeacherConflictsAsync(
            int teacherId,
            DateTime sessionDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            using var conn = _factory.Create();
            return await conn.QueryAsync<TeacherSessionConflict>(
                "dbo.CourseSessions_GetTeacherConflicts",
                new
                {
                    TeacherID = teacherId,
                    SessionDate = sessionDate,
                    StartTime = startTime,
                    EndTime = endTime
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> UpdateAsync(CourseSession session)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.CourseSessions_Update",
                new
                {
                    session.SessionID,
                    session.SessionDate,
                    session.StartTime,
                    session.EndTime,
                    session.IsCancelled
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteAsync(int sessionId)
        {
            using var conn = _factory.Create();
            return await conn.ExecuteScalarAsync<int>(
                "dbo.CourseSessions_Delete",
                new { SessionID = sessionId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
