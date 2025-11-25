using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs;
using SchoolManagementSystem.Core.DTOs.Student;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public partial class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentService studentService,
            ILogger<StudentController> logger,
            INotificationService notificationService)
        {
            _studentService = studentService;
            _logger = logger;
            _notificationService = notificationService;
        }

        private int GetCurrentStudentId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Get student dashboard with statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var studentId = GetCurrentStudentId();
            var dashboard = await _studentService.GetDashboardAsync(studentId);

            return Ok(new
            {
                success = true,
                message = "Dashboard retrieved successfully",
                data = dashboard
            });
        }

        /// <summary>
        /// Get all enrolled classes
        /// </summary>
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var studentId = GetCurrentStudentId();
            var classes = await _studentService.GetEnrolledClassesAsync(studentId);

            return Ok(new
            {
                success = true,
                message = "Classes retrieved successfully",
                data = classes,
                totalCount = classes.Count
            });
        }

        /// <summary>
        /// Get attendance records
        /// </summary>
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendance([FromQuery] int? classId = null)
        {
            var studentId = GetCurrentStudentId();
            var attendance = await _studentService.GetAttendanceAsync(studentId, classId);

            return Ok(new
            {
                success = true,
                message = "Attendance retrieved successfully",
                data = attendance,
                totalRecords = attendance.Count
            });
        }

        /// <summary>
        /// Get all graded submissions
        /// </summary>
        [HttpGet("grades")]
        public async Task<IActionResult> GetGrades()
        {
            var studentId = GetCurrentStudentId();
            var grades = await _studentService.GetGradesAsync(studentId);

            var averageGrade = grades.Any()
                ? Math.Round(grades.Average(g => g.Grade), 2)
                : (decimal?)null;

            return Ok(new
            {
                success = true,
                message = "Grades retrieved successfully",
                data = grades,
                totalGraded = grades.Count,
                averageGrade = averageGrade
            });
        }

        /// <summary>
        /// Get assignments for enrolled classes
        /// </summary>
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments(
            [FromQuery] int? classId = null,
            [FromQuery] bool? onlyPending = null)
        {
            var studentId = GetCurrentStudentId();
            var assignments = await _studentService.GetAssignmentsAsync(studentId, classId, onlyPending);

            return Ok(new
            {
                success = true,
                message = "Assignments retrieved successfully",
                data = assignments,
                totalCount = assignments.Count,
                pendingCount = assignments.Count(a => !a.IsSubmitted),
                submittedCount = assignments.Count(a => a.IsSubmitted),
                overdueCount = assignments.Count(a => a.IsOverdue)
            });
        }

        /// <summary>
        /// Submit an assignment
        /// </summary>
        [HttpPost("assignments/{id}/submit")]
        public async Task<IActionResult> SubmitAssignment(
            int id,
            [FromBody] SubmitAssignmentDto dto)
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

            var studentId = GetCurrentStudentId();
            var submission = await _studentService.SubmitAssignmentAsync(id, dto, studentId);

            _logger.LogInformation(
                "Assignment submitted by student {StudentId}: AssignmentId {AssignmentId}",
                studentId,
                id);

            return Ok(new
            {
                success = true,
                message = "Assignment submitted successfully",
                data = submission
            });
        }
    }
    public partial class StudentController
    {
        /// <summary>
        /// Submit an assignment with file upload
        /// </summary>
        [HttpPost("assignments/{id}/submit-file")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<IActionResult> SubmitAssignmentWithFile(
            int id,
            [FromForm] FileUploadDto file)
        {
            if (file == null || file.File.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "File is required"
                });
            }

            var studentId = GetCurrentStudentId();
            var submission = await _studentService.SubmitAssignmentWithFileAsync(id, file.File, studentId);

            _logger.LogInformation(
                "Assignment submitted with file by student {StudentId}: AssignmentId {AssignmentId}",
                studentId,
                id);

            return Ok(new
            {
                success = true,
                message = "Assignment submitted successfully",
                data = submission
            });
        }

        /// <summary>
        /// Download submission file
        /// </summary>
        [HttpGet("submissions/{submissionId}/download")]
        public async Task<IActionResult> DownloadSubmission(int submissionId)
        {
            var studentId = GetCurrentStudentId();

            var result = await _studentService.GetSubmissionFileAsync(submissionId, studentId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Submission or file not found"
                });
            }

            return File(result.Value.fileData, "application/octet-stream", result.Value.fileName);
        }
        /// <summary>
        /// Get all notifications for current student
        /// </summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool? onlyUnread = null)
        {
            var studentId = GetCurrentStudentId();
            var notifications = await _notificationService.GetStudentNotificationsAsync(studentId, onlyUnread);

            return Ok(new
            {
                success = true,
                message = "Notifications retrieved successfully",
                data = notifications,
                totalCount = notifications.Count,
                unreadCount = notifications.Count(n => !n.IsRead)
            });
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        [HttpGet("notifications/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var studentId = GetCurrentStudentId();
            var count = await _notificationService.GetUnreadCountAsync(studentId);

            return Ok(new
            {
                success = true,
                unreadCount = count
            });
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var studentId = GetCurrentStudentId();
            var notification = await _notificationService.MarkAsReadAsync(id, studentId);

            _logger.LogInformation(
                "Notification marked as read by student {StudentId}: NotificationId {NotificationId}",
                studentId,
                id);

            return Ok(new
            {
                success = true,
                message = "Notification marked as read",
                data = notification
            });
        }
    }
}
