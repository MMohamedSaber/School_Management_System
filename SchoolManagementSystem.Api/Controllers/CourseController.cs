using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Course;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(
            ICourseService courseService,
            ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        /// <summary>
        /// Get all courses with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] int? minCredits = null,
            [FromQuery] int? maxCredits = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await _courseService.GetAllAsync(
                pageNumber,
                pageSize,
                searchTerm,
                departmentId,
                minCredits,
                maxCredits);

            return Ok(new
            {
                success = true,
                message = "Courses retrieved successfully",
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
        /// Get course by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);

            return Ok(new
            {
                success = true,
                message = "Course retrieved successfully",
                data = course
            });
        }

        /// <summary>
        /// Create a new course
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
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

            var course = await _courseService.CreateAsync(dto);

            _logger.LogInformation("Course created: {Code} - {Name}", course.Code, course.Name);

            return CreatedAtAction(
                nameof(GetById),
                new { id = course.Id },
                new
                {
                    success = true,
                    message = "Course created successfully",
                    data = course
                });
        }

        /// <summary>
        /// Update an existing course
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
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

            var course = await _courseService.UpdateAsync(id, dto);

            _logger.LogInformation("Course updated: {CourseId}", id);

            return Ok(new
            {
                success = true,
                message = "Course updated successfully",
                data = course
            });
        }

        /// <summary>
        /// Soft delete a course
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteAsync(id);

            _logger.LogInformation("Course deleted: {CourseId}", id);

            return Ok(new
            {
                success = true,
                message = "Course deleted successfully"
            });
        }
    }
}
