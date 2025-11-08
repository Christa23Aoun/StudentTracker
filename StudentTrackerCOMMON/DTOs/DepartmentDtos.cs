using System.ComponentModel.DataAnnotations;

namespace StudentTrackerCOMMON.DTOs
{
 
    public class DepartmentCreateDto
    {
        [Required(ErrorMessage = "Department name is required.")]
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

   
    public class DepartmentUpdateDto
    {
        [Required]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
