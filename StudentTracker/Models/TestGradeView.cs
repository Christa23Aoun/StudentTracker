using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class TestGradeView
    {
        public string StudentName { get; set; } = string.Empty;

        public int TestGradeID { get; set; }

        [Required(ErrorMessage = "Please select a test.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid test.")]
        public int TestID { get; set; }

        [Required(ErrorMessage = "Please select a student.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid student.")]
        public int StudentID { get; set; }

        [Required(ErrorMessage = "Please enter a score.")]
        [Range(0.0, 100.0, ErrorMessage = "Score must be between 0 and 100.")]
        public decimal Score { get; set; }

        public bool IsValidated { get; set; }

        public string? TestName { get; set; }
        public string? CourseName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid course.")]
        public int CourseID { get; set; }
    }
}
