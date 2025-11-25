
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Notification
{
    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; }

        public int? ClassId { get; set; } // Optional: send to specific class

        public List<int> StudentIds { get; set; } // Optional: send to specific students

    }
}
