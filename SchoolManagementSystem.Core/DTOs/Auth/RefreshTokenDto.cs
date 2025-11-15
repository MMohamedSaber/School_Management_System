using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
