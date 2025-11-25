using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Notification;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/teacher/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private int GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Send notification to all students in a class
        /// </summary>
        [HttpPost("send-to-class/{classId}")]
        public async Task<IActionResult> SendToClass(
            int classId,
            [FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            var teacherId = GetCurrentTeacherId();
            var notifications = await _notificationService.SendToClassAsync(classId, dto, teacherId);

            _logger.LogInformation(
                "Teacher {TeacherId} sent notification to class {ClassId}: {NotificationCount} students",
                teacherId,
                classId,
                notifications.Count);

            return Ok(new
            {
                success = true,
                message = $"Notification sent to {notifications.Count} students",
                data = notifications
            });
        }

        /// <summary>
        /// Send notification to specific students
        /// </summary>
        [HttpPost("send-to-students")]
        public async Task<IActionResult> SendToStudents([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            if (dto.StudentIds == null || !dto.StudentIds.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "At least one student ID is required"
                });
            }

            var teacherId = GetCurrentTeacherId();
            var notifications = await _notificationService.SendToStudentsAsync(dto.StudentIds, dto, teacherId);

            _logger.LogInformation(
                "Teacher {TeacherId} sent notification to {StudentCount} students",
                teacherId,
                notifications.Count);

            return Ok(new
            {
                success = true,
                message = $"Notification sent to {notifications.Count} students",
                data = notifications
            });
        }
    }
}
