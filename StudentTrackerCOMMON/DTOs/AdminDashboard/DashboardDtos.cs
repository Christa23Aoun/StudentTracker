namespace StudentTrackerCOMMON.DTOs
{
    public record AdminDashboardSummaryDto(
        int TotalStudents,
        int TotalTeachers,
        int ActiveCourses,
        int Departments,
        string CurrentAcademicYear,
        string CurrentSemester
    );

    public record DepartmentSummaryDto(int DepartmentId, string DepartmentName, int CourseCount);

    public record CourseAdminDto(int CourseId, string CourseName, string Department, string Teacher, bool IsActive);

    public record UserSummaryDto(int UserId, string FullName, string Email, string Role, bool IsActive);

    public record PendingGradeDto(
        int GradeId,
        int StudentId,
        string StudentName,
        int CourseId,
        string CourseName,
        int TestId,
        string TestName,
        decimal Score,
        decimal MaxScore,
        bool IsSubmitted
    );

    public record ActionResultDto(bool Success, string Message);
}
