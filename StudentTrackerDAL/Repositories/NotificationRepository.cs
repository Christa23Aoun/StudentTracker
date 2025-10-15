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
            var id = await con.ExecuteScalarAsync<int>(
                "dbo.sp_Notification_Create",
                new
                {
                    notification.UserID,
                    notification.Message,
                    notification.Type
                },
                commandType: CommandType.StoredProcedure);
            return id;
        }

        public async Task<IEnumerable<Notification>> GetForUserAsync(int userId)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<Notification>(
                "dbo.sp_Notification_GetForUser",
                new { UserID = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            using var con = new SqlConnection(_connectionString);
            var rows = await con.ExecuteAsync(
                "UPDATE dbo.Notifications SET IsRead = 1 WHERE NotificationID = @Id",
                new { Id = notificationId });
            return rows > 0;
        }
    }
}
