namespace StudentTracker.Models
{
    public class NotificationView
    {
        public int NotificationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
