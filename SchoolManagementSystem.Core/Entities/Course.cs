using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        public int Credits { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Department Department { get; set; }
        public ICollection<Class> Classes { get; set; }
    }
}

