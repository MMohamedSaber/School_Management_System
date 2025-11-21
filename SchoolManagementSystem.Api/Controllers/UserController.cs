using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.User;
using SchoolManagementSystem.Core.Interfaces;
using System.Security.Claims;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] int? role = null,
            [FromQuery] bool? isActive = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var filter = new UserFilterDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                Role = role,
                IsActive = isActive
            };

            var result = await _userService.GetAllAsync(filter);

            return Ok(new
            {
                success = true,
                message = "Users retrieved successfully",
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
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            return Ok(new
            {
                success = true,
                message = "User retrieved successfully",
                data = user
            });
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _userService.GetUserStatisticsAsync();

            return Ok(new
            {
                success = true,
                message = "Statistics retrieved successfully",
                data = stats
            });
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
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

            var user = await _userService.UpdateAsync(id, dto);

            _logger.LogInformation("User updated: {UserId}", id);

            return Ok(new
            {
                success = true,
                message = "User updated successfully",
                data = user
            });
        }

        /// <summary>
        /// Soft delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Prevent admin from deleting themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == id)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "You cannot delete your own account"
                });
            }

            await _userService.DeleteAsync(id);

            _logger.LogInformation("User deleted: {UserId}", id);

            return Ok(new
            {
                success = true,
                message = "User deleted successfully"
            });
        }
    }
}
