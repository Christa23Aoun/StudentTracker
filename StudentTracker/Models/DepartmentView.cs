using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class DepartmentView
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
