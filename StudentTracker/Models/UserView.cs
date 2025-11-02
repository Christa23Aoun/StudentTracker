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

        // 🔹 Keep this for compatibility with API (hashed password in DB)
        public string PasswordHash { get; set; } = string.Empty;

        // 🔹 Add this for front-end login / register form binding
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // 🔹 Numeric role ID (1 = Admin, 2 = Teacher, 3 = Student)
        [Required]
        public int RoleID { get; set; }

        // 🔹 Optional string role name (used in session & redirects)
        public string? Role { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
