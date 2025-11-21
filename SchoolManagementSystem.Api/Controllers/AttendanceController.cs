using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Attendance;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/teacher/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IAttendanceService attendanceService,
            ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        private int GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Mark attendance for a single student
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
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
            var attendance = await _attendanceService.MarkAttendanceAsync(dto, teacherId);

            _logger.LogInformation(
                "Attendance marked by teacher {TeacherId} for student {StudentId} in class {ClassId}",
                teacherId,
                dto.StudentId,
                dto.ClassId);

            return Ok(new
            {
                success = true,
                message = "Attendance marked successfully",
                data = attendance
            });
        }

        /// <summary>
        /// Mark attendance for multiple students at once
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkMarkAttendance([FromBody] BulkMarkAttendanceDto dto)
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
            var attendances = await _attendanceService.BulkMarkAttendanceAsync(dto, teacherId);

            _logger.LogInformation(
                "Bulk attendance marked by teacher {TeacherId} for class {ClassId} - {Count} students",
                teacherId,
                dto.ClassId,
                attendances.Count);

            return Ok(new
            {
                success = true,
                message = $"Attendance marked for {attendances.Count} students",
                data = attendances
            });
        }

        /// <summary>
        /// Get attendance history for a class with filters
        /// </summary>
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassAttendance(
            int classId,
            [FromQuery] int? studentId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var filter = new AttendanceFilterDto
            {
                ClassId = classId,
                StudentId = studentId,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var teacherId = GetCurrentTeacherId();
            var result = await _attendanceService.GetClassAttendanceAsync(filter, teacherId);

            return Ok(new
            {
                success = true,
                message = "Attendance retrieved successfully",
                data = result.Items,
                pagination = new
                {
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                }
            });
        }

        /// <summary>
        /// Get attendance summary for a class
        /// </summary>
        [HttpGet("{classId}/summary")]
        public async Task<IActionResult> GetClassAttendanceSummary(int classId)
        {
            var teacherId = GetCurrentTeacherId();
            var summary = await _attendanceService.GetClassAttendanceSummaryAsync(classId, teacherId);

            return Ok(new
            {
                success = true,
                message = "Attendance summary retrieved successfully",
                data = summary
            });
        }

        /// <summary>
        /// Get attendance for a specific student in a class
        /// </summary>
        [HttpGet("{classId}/student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int classId, int studentId)
        {
            var teacherId = GetCurrentTeacherId();
            var attendances = await _attendanceService.GetStudentAttendanceAsync(classId, studentId, teacherId);

            return Ok(new
            {
                success = true,
                message = "Student attendance retrieved successfully",
                data = attendances,
                totalRecords = attendances.Count
            });
        }
    }
}
