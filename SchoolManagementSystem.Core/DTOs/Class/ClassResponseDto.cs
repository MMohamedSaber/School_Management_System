namespace SchoolManagementSystem.Core.DTOs.Class
{
    public class ClassResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int EnrolledStudentsCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
