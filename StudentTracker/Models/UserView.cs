using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class UserView
    {
        public int UserID { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        public int RoleID { get; set; }

        public string? Role { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
