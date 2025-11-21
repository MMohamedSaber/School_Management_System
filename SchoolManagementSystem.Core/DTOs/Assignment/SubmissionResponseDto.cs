namespace SchoolManagementSystem.Core.DTOs.Assignment
{
    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string FileUrl { get; set; }
        public decimal? Grade { get; set; }
        public int? GradedByTeacherId { get; set; }
        public string GradedByTeacherName { get; set; }
        public string Remarks { get; set; }
        public bool IsGraded { get; set; }
    }
}
