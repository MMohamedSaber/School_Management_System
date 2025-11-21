namespace SchoolManagementSystem.Core.DTOs.Attendance
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int MarkedByTeacherId { get; set; }
        public string MarkedByTeacherName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
