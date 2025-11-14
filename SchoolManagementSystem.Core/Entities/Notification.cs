using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public UserRole? RecipientRole { get; set; }

        public int? RecipientId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}

