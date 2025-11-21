namespace SchoolManagementSystem.Core.DTOs.Class
{
    public class StudentEnrollmentResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
