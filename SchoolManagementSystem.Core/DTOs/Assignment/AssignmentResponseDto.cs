namespace SchoolManagementSystem.Core.DTOs.Assignment
{
    public class AssignmentResponseDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int CreatedByTeacherId { get; set; }
        public string CreatedByTeacherName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
    }
}
