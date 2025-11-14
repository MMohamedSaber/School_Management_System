using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [ForeignKey("HeadOfDepartment")]
        public int? HeadOfDepartmentId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public User HeadOfDepartment { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}

