using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class CourseView
    {
        public int CourseID { get; set; }

        [StringLength(30)]
        public string? CourseCode { get; set; }

        [Required, StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [Range(1, 10)]
        public int CreditHours { get; set; } = 3;

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        public int SemesterID { get; set; }

        [Required]
        public int TeacherID { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

   
        public IEnumerable<LookupItem> Departments { get; set; } = new List<LookupItem>();
        public IEnumerable<LookupItem> Semesters { get; set; } = new List<LookupItem>();

       
        public string? DepartmentName { get; set; }
        public string? SemesterName { get; set; }
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
