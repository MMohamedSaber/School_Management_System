using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [ForeignKey("MarkedByTeacher")]
        public int MarkedByTeacherId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Class Class { get; set; }
        public User Student { get; set; }
        public User MarkedByTeacher { get; set; }
    }
}

