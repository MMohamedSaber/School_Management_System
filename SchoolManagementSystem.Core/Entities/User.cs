using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } // Hashed

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // For soft delete

        // Navigation Properties
        public ICollection<Class> TaughtClasses { get; set; }
        public ICollection<Department> HeadedDepartments { get; set; }
        public ICollection<StudentClass> StudentClasses { get; set; }
        public ICollection<Attendance> StudentAttendances { get; set; }
        public ICollection<Attendance> MarkedAttendances { get; set; }
        public ICollection<Assignment> CreatedAssignments { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<Submission> GradedSubmissions { get; set; }
    }
}

