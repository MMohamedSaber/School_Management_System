
namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class StudentAssignmentDto
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string CourseCode { get; set; }
        public string TeacherName { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public decimal? Grade { get; set; }
        public bool IsOverdue { get; set; }
    }
}
