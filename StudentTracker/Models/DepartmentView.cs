using System;
using System.ComponentModel.DataAnnotations;

namespace StudentTracker.Models
{
    public class DepartmentView
    {
        public int DepartmentID { get; set; }

        [Required, StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
