using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using StudentTrackerCOMMON.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace StudentTrackerDAL.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<int> CreateAsync(Notification notification)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var id = await con.ExecuteScalarAsync<int>(
                "dbo.sp_Notification_Create",
                new
                {
                    notification.UserID,
                    notification.Message,
                    notification.Type,
                    notification.TargetUrl
                },
                commandType: CommandType.StoredProcedure);

            await con.ExecuteAsync(
                "dbo.sp_AuditLog_Add",
                new
                {
                    UserID = notification.UserID,
                    Action = "INSERT",
                    TableName = "Notifications",
                    RecordID = id.ToString(),
                    OldValue = (string?)null,
                    NewValue = notification.Message
                },
                commandType: CommandType.StoredProcedure);

            return id;
        }

        public async Task<IEnumerable<Notification>> GetForUserAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);

            var notifications = await con.QueryAsync<Notification>(
                "dbo.sp_Notification_GetForUser",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure);

            await con.ExecuteAsync(
                "UPDATE dbo.Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0",
                new { UserID = userId });

            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0",
                new { UserID = userId });
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            using var con = new SqlConnection(_connectionString);
            var rows = await con.ExecuteAsync(
                "UPDATE dbo.Notifications SET IsRead = 1 WHERE NotificationID = @Id",
                new { Id = notificationId });
            return rows > 0;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);

            return await con.ExecuteAsync(
                "UPDATE dbo.Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0",
                new { UserID = userId }
            );
        }
    }
}
