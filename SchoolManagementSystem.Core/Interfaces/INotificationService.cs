
using SchoolManagementSystem.Core.DTOs.Notification;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface INotificationService
    {
        // Student methods
        Task<List<NotificationResponseDto>> GetStudentNotificationsAsync(int studentId, bool? onlyUnread = null);
        Task<NotificationResponseDto> MarkAsReadAsync(int notificationId, int studentId);
        Task<int> GetUnreadCountAsync(int studentId);

        // Teacher methods
        Task<List<NotificationResponseDto>> SendToClassAsync(int classId, CreateNotificationDto dto, int teacherId);
        Task<List<NotificationResponseDto>> SendToStudentsAsync(List<int> studentIds, CreateNotificationDto dto, int teacherId);
    }
}
