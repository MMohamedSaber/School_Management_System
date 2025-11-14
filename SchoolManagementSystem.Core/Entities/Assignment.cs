using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("CreatedByTeacher")]
        public int CreatedByTeacherId { get; set; }

        // Navigation Properties
        public Class Class { get; set; }
        public User CreatedByTeacher { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}

