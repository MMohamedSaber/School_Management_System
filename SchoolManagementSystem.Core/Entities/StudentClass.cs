using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Core.Entities
{
    // 5. StudentClass.cs (Mapping Table)



    public class StudentClass
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Class")]
        public int ClassId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User Student { get; set; }
        public Class Class { get; set; }
    }
}

