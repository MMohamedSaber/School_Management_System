
namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class StudentAttendanceDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string MarkedByTeacherName { get; set; }
    }
}
