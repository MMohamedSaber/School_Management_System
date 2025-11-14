using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }

        [MaxLength(50)]
        public string Semester { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Course Course { get; set; }
        public User Teacher { get; set; }
        public ICollection<StudentClass> StudentClasses { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
    }
}

