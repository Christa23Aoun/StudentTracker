using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace StudentTracker.Models
{
    public class CourseView
    {
        public int CourseID { get; set; }

        [StringLength(30)]
        public string? CourseCode { get; set; }

        public string? TeacherName { get; set; }

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
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }
        public int StudentCount { get; set; }
        public List<StudentView> Students { get; set; } = new();

        public IEnumerable<LookupItem> Departments { get; set; } = new List<LookupItem>();
        public IEnumerable<LookupItem> Semesters { get; set; } = new List<LookupItem>();
        public IEnumerable<LookupItem> Teachers { get; set; } = new List<LookupItem>();

        public string? DepartmentName { get; set; }
        public string? SemesterName { get; set; }
    }

    public class StudentView
    {
        public string StudentName { get; set; } = string.Empty;
        public double AverageGrade { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class LookupItem
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonProperty("DepartmentID")]
        private int DepartmentID { set => Id = value; }

        [JsonProperty("SemesterID")]
        private int SemesterID { set => Id = value; }

        [JsonProperty("UserID")]
        private int UserID { set => Id = value; }
    }
}
