
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Class
{
    public class UpdateClassDto
    {
        [Required(ErrorMessage = "Class name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [MaxLength(50, ErrorMessage = "Semester cannot exceed 50 characters")]
        public string Semester { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
    }

}
