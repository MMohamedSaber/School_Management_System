
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Class
{
    public class EnrollStudentDto
    {
        [Required(ErrorMessage = "Student ID is required")]
        public int StudentId { get; set; }
    }
}
