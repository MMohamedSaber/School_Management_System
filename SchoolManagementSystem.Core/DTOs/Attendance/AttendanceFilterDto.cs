namespace SchoolManagementSystem.Core.DTOs.Attendance
{
    public class AttendanceFilterDto
    {
        public int ClassId { get; set; }
        public int? StudentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; } // 1=Present, 2=Absent, 3=Late
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
