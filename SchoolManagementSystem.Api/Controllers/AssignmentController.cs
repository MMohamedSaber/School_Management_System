using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Assignment;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/teacher/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(
            IAssignmentService assignmentService,
            ILogger<AssignmentsController> logger)
        {
            _assignmentService = assignmentService;
            _logger = logger;
        }

        private int GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Create a new assignment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
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
            var assignment = await _assignmentService.CreateAsync(dto, teacherId);

            _logger.LogInformation(
                "Assignment created by teacher {TeacherId}: {AssignmentTitle}",
                teacherId,
                assignment.Title);

            return CreatedAtAction(
                nameof(GetById),
                new { id = assignment.Id },
                new
                {
                    success = true,
                    message = "Assignment created successfully",
                    data = assignment
                });
        }

        /// <summary>
        /// Update an assignment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAssignmentDto dto)
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
            var assignment = await _assignmentService.UpdateAsync(id, dto, teacherId);

            _logger.LogInformation("Assignment updated: {AssignmentId}", id);

            return Ok(new
            {
                success = true,
                message = "Assignment updated successfully",
                data = assignment
            });
        }

        /// <summary>
        /// Delete an assignment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacherId = GetCurrentTeacherId();
            await _assignmentService.DeleteAsync(id, teacherId);

            _logger.LogInformation("Assignment deleted: {AssignmentId}", id);

            return Ok(new
            {
                success = true,
                message = "Assignment deleted successfully"
            });
        }

        /// <summary>
        /// Get all assignments for a class
        /// </summary>
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetClassAssignments(
            int classId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var teacherId = GetCurrentTeacherId();
            var result = await _assignmentService.GetClassAssignmentsAsync(
                classId,
                teacherId,
                pageNumber,
                pageSize);

            return Ok(new
            {
                success = true,
                message = "Assignments retrieved successfully",
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
        /// Get assignment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacherId = GetCurrentTeacherId();
            var assignment = await _assignmentService.GetByIdAsync(id, teacherId);

            return Ok(new
            {
                success = true,
                message = "Assignment retrieved successfully",
                data = assignment
            });
        }

        /// <summary>
        /// Get all submissions for an assignment
        /// </summary>
        [HttpGet("{id}/submissions")]
        public async Task<IActionResult> GetSubmissions(int id)
        {
            var teacherId = GetCurrentTeacherId();
            var submissions = await _assignmentService.GetAssignmentSubmissionsAsync(id, teacherId);

            return Ok(new
            {
                success = true,
                message = "Submissions retrieved successfully",
                data = submissions,
                totalSubmissions = submissions.Count,
                gradedCount = submissions.Count(s => s.IsGraded),
                pendingCount = submissions.Count(s => !s.IsGraded)
            });
        }

        /// <summary>
        /// Grade a student submission
        /// </summary>
        [HttpPost("submissions/{submissionId}/grade")]
        public async Task<IActionResult> GradeSubmission(
            int submissionId,
            [FromBody] GradeSubmissionDto dto)
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
            var submission = await _assignmentService.GradeSubmissionAsync(submissionId, dto, teacherId);

            _logger.LogInformation(
                "Submission graded by teacher {TeacherId}: SubmissionId {SubmissionId}, Grade {Grade}",
                teacherId,
                submissionId,
                dto.Grade);

            return Ok(new
            {
                success = true,
                message = "Submission graded successfully",
                data = submission
            });
        }
    }
}
