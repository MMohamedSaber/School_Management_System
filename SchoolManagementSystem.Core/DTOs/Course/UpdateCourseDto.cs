
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Course
{
    public class UpdateCourseDto
    {
        [Required(ErrorMessage = "Course name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        public string Code { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Department ID is required")]
        public int DepartmentId { get; set; }

        [Range(1, 20, ErrorMessage = "Credits must be between 1 and 20")]
        public int Credits { get; set; }
    }
}
