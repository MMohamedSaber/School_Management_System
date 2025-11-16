using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Department
{
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public int? HeadOfDepartmentId { get; set; }
    }
}
