using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.User
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int Role { get; set; } // 1=Admin, 2=Teacher, 3=Student

        // Optional: Update password
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        public string NewPassword { get; set; }
    }
}
