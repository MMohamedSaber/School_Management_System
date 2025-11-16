using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(
            IDepartmentService departmentService,
            ILogger<DepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all departments with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search by name or description</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                // Validate pagination
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var result = await _departmentService.GetAllAsync(pageNumber, pageSize, searchTerm);

                return Ok(new
                {
                    success = true,
                    message = "Departments retrieved successfully",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving departments"
                });
            }
        }

        /// <summary>
        /// Get department by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Department retrieved successfully",
                    data = department
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Department not found: {Message}", ex.Message);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department with ID {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the department"
                });
            }
        }

        /// <summary>
        /// Create a new department
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            try
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

                var department = await _departmentService.CreateAsync(dto);

                _logger.LogInformation("Department created: {DepartmentName}", department.Name);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = department.Id },
                    new
                    {
                        success = true,
                        message = "Department created successfully",
                        data = department
                    });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Department creation failed: {Message}", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the department"
                });
            }
        }

        /// <summary>
        /// Update an existing department
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto dto)
        {
            try
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

                var department = await _departmentService.UpdateAsync(id, dto);

                _logger.LogInformation("Department updated: {DepartmentId}", id);

                return Ok(new
                {
                    success = true,
                    message = "Department updated successfully",
                    data = department
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Department not found: {Message}", ex.Message);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Department update failed: {Message}", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department with ID {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the department"
                });
            }
        }

        /// <summary>
        /// Soft delete a department
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _departmentService.DeleteAsync(id);

                _logger.LogInformation("Department soft deleted: {DepartmentId}", id);

                return Ok(new
                {
                    success = true,
                    message = "Department deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Department not found: {Message}", ex.Message);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Department deletion failed: {Message}", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department with ID {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the department"
                });
            }
        }
    }
}