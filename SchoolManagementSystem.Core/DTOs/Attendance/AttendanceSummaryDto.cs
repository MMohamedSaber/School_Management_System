namespace SchoolManagementSystem.Core.DTOs.Attendance
{
    public class AttendanceSummaryDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int TotalStudents { get; set; }
        public int TotalSessions { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}
