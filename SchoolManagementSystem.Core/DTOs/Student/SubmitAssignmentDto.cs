
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class SubmitAssignmentDto
    {
        [Required(ErrorMessage = "File URL is required")]
        [MaxLength(500, ErrorMessage = "File URL cannot exceed 500 characters")]
        public string FileUrl { get; set; }
    }
}
