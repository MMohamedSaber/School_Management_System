namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class StudentDashboardDto
    {
        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int TotalAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int SubmittedAssignments { get; set; }
        public int GradedAssignments { get; set; }
        public decimal? AverageGrade { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}
