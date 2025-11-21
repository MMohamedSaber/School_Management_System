using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Class;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.API.Controllers.Teacher
{
    [ApiController]
    [Route("api/teacher/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(
            IClassService classService,
            ILogger<ClassesController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        private int GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// Get all classes for current teacher
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyClasses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var teacherId = GetCurrentTeacherId();
            var result = await _classService.GetTeacherClassesAsync(teacherId, pageNumber, pageSize, searchTerm);

            return Ok(new
            {
                success = true,
                message = "Classes retrieved successfully",
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
        /// Get class by ID (only teacher's own classes)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacherId = GetCurrentTeacherId();
            var classEntity = await _classService.GetByIdAsync(id, teacherId);

            return Ok(new
            {
                success = true,
                message = "Class retrieved successfully",
                data = classEntity
            });
        }

        /// <summary>
        /// Create a new class
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
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
            var classEntity = await _classService.CreateAsync(dto, teacherId);

            _logger.LogInformation("Class created by teacher {TeacherId}: {ClassName}", teacherId, classEntity.Name);

            return CreatedAtAction(
                nameof(GetById),
                new { id = classEntity.Id },
                new
                {
                    success = true,
                    message = "Class created successfully",
                    data = classEntity
                });
        }

        /// <summary>
        /// Update a class
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
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
            var classEntity = await _classService.UpdateAsync(id, dto, teacherId);

            _logger.LogInformation("Class updated by teacher {TeacherId}: {ClassId}", teacherId, id);

            return Ok(new
            {
                success = true,
                message = "Class updated successfully",
                data = classEntity
            });
        }

        /// <summary>
        /// Deactivate a class
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var teacherId = GetCurrentTeacherId();
            var classEntity = await _classService.DeactivateAsync(id, teacherId);

            _logger.LogInformation("Class deactivated by teacher {TeacherId}: {ClassId}", teacherId, id);

            return Ok(new
            {
                success = true,
                message = "Class deactivated successfully",
                data = classEntity
            });
        }

        /// <summary>
        /// Enroll a student in the class
        /// </summary>
        [HttpPost("{id}/enroll-student")]
        public async Task<IActionResult> EnrollStudent(int id, [FromBody] EnrollStudentDto dto)
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
            var enrollment = await _classService.EnrollStudentAsync(id, dto, teacherId);

            _logger.LogInformation(
                "Student {StudentId} enrolled in class {ClassId} by teacher {TeacherId}",
                dto.StudentId,
                id,
                teacherId);

            return Ok(new
            {
                success = true,
                message = "Student enrolled successfully",
                data = enrollment
            });
        }

        /// <summary>
        /// Get all students enrolled in a class
        /// </summary>
        [HttpGet("{id}/students")]
        public async Task<IActionResult> GetClassStudents(int id)
        {
            var teacherId = GetCurrentTeacherId();
            var students = await _classService.GetClassStudentsAsync(id, teacherId);

            return Ok(new
            {
                success = true,
                message = "Students retrieved successfully",
                data = students,
                totalCount = students.Count
            });
        }
    }
}