
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Notification;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // STUDENT METHODS
        // ============================================================

        public async Task<List<NotificationResponseDto>> GetStudentNotificationsAsync(
            int studentId,
            bool? onlyUnread = null)
        {
            var query = _context.Notifications
                .Where(n =>
                    (n.RecipientId == studentId || n.RecipientRole == UserRole.Student) &&
                    n.RecipientId == studentId)
                .AsQueryable();

            if (onlyUnread.HasValue && onlyUnread.Value)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    RecipientRole = n.RecipientRole.HasValue ? n.RecipientRole.ToString() : null,
                    RecipientId = n.RecipientId,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return notifications;
        }

        public async Task<NotificationResponseDto> MarkAsReadAsync(int notificationId, int studentId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == studentId);

            if (notification == null)
            {
                throw new KeyNotFoundException("Notification not found");
            }

            if (notification.IsRead)
            {
                throw new InvalidOperationException("Notification is already marked as read");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                RecipientRole = notification.RecipientRole.HasValue ? notification.RecipientRole.ToString() : null,
                RecipientId = notification.RecipientId,
                CreatedDate = notification.CreatedDate,
                IsRead = notification.IsRead
            };
        }

        public async Task<int> GetUnreadCountAsync(int studentId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == studentId && !n.IsRead);
        }

        // ============================================================
        // TEACHER METHODS
        // ============================================================

        public async Task<List<NotificationResponseDto>> SendToClassAsync(
            int classId,
            CreateNotificationDto dto,
            int teacherId)
        {
            // Validate class exists and teacher owns it
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId && c.IsActive);

            if (classEntity == null)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            // Get all enrolled students
            var studentIds = await _context.StudentClasses
                .Where(sc => sc.ClassId == classId)
                .Select(sc => sc.StudentId)
                .ToListAsync();

            if (!studentIds.Any())
            {
                throw new InvalidOperationException("No students enrolled in this class");
            }

            // Create notifications for all students
            var notifications = new List<Core.Entities.Notification>();

            foreach (var studentId in studentIds)
            {
                var notification = new Core.Entities.Notification
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    RecipientRole = UserRole.Student,
                    RecipientId = studentId,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                notifications.Add(notification);
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                RecipientRole = n.RecipientRole.HasValue ? n.RecipientRole.ToString() : null,
                RecipientId = n.RecipientId,
                CreatedDate = n.CreatedDate,
                IsRead = n.IsRead
            }).ToList();
        }

        public async Task<List<NotificationResponseDto>> SendToStudentsAsync(
            List<int> studentIds,
            CreateNotificationDto dto,
            int teacherId)
        {
            if (studentIds == null || !studentIds.Any())
            {
                throw new InvalidOperationException("At least one student ID is required");
            }

            // Validate students exist and are actually students
            var validStudents = await _context.Users
                .Where(u => studentIds.Contains(u.Id) && u.Role == UserRole.Student && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            if (!validStudents.Any())
            {
                throw new InvalidOperationException("No valid students found");
            }

            var invalidStudentIds = studentIds.Except(validStudents).ToList();
            if (invalidStudentIds.Any())
            {
                throw new InvalidOperationException(
                    $"Invalid student IDs: {string.Join(", ", invalidStudentIds)}");
            }

            // Create notifications
            var notifications = new List<Core.Entities.Notification>();

            foreach (var studentId in validStudents)
            {
                var notification = new Core.Entities.Notification
                {
                    Title = dto.Title,
                    Message = dto.Message,
                    RecipientRole = UserRole.Student,
                    RecipientId = studentId,
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                notifications.Add(notification);
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                RecipientRole = n.RecipientRole.HasValue ? n.RecipientRole.ToString() : null,
                RecipientId = n.RecipientId,
                CreatedDate = n.CreatedDate,
                IsRead = n.IsRead
            }).ToList();
        }
    }
}
