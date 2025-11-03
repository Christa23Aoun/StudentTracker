using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class LoginView
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterView
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // 🔹 Numeric role ID to match the API field
        [Required]
        public int RoleID { get; set; }

        // Optional helper property for display
        public string? RoleName =>
            RoleID == 1 ? "Admin" :
            RoleID == 2 ? "Teacher" :
            RoleID == 3 ? "Student" : "Unknown";
    }
}
