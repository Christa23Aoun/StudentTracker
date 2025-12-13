using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class LoginView
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }


        public string Role { get; set; } = string.Empty;
    }

    public class RegisterView
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

        public string? RoleName =>
            RoleID == 1 ? "Admin" :
            RoleID == 2 ? "Teacher" :
            RoleID == 3 ? "Student" : "Unknown";
        [Required]
        public string Role { get; set; } = string.Empty;

    }
}
