namespace StudentTracker.Models
{
    public class NotificationView
    {
        public int NotificationID { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
