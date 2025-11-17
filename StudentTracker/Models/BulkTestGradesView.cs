using System.Collections.Generic;

namespace StudentTracker.Models
{
    public class StudentGradeInput
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public double? Score { get; set; }
        public bool IsValidated { get; set; }
    }

    public class BulkTestGradesView
    {
        public int CourseID { get; set; }
        public int SelectedTestID { get; set; }
        public List<TestView> AvailableTests { get; set; } = new List<TestView>();
        public List<StudentGradeInput> Students { get; set; } = new List<StudentGradeInput>();
    }
}
