namespace SchoolManagementSystem.Core.DTOs.Notification
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecipientRole { get; set; }
        public int? RecipientId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}
