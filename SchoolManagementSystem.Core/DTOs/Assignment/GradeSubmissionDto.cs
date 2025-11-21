
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Assignment
{
    public class GradeSubmissionDto
    {
        [Required(ErrorMessage = "Grade is required")]
        [Range(0, 100, ErrorMessage = "Grade must be between 0 and 100")]
        public decimal Grade { get; set; }

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string Remarks { get; set; }
    }
}
