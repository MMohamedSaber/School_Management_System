
namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class StudentGradeDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string ClassName { get; set; }
        public string CourseCode { get; set; }
        public DateTime SubmittedDate { get; set; }
        public decimal Grade { get; set; }
        public string Remarks { get; set; }
        public string GradedByTeacherName { get; set; }
    }
}
