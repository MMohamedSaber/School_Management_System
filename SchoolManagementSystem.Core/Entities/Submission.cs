using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string FileUrl { get; set; }

        public decimal? Grade { get; set; }

        [ForeignKey("GradedByTeacher")]
        public int? GradedByTeacherId { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        // Navigation Properties
        public Assignment Assignment { get; set; }
        public User Student { get; set; }
        public User GradedByTeacher { get; set; }
    }
}

