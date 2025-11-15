// API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.DTOs.Auth;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>
        /// Role must be one of the following:
        /// - Admin = 1
        /// - Teacher = 2
        /// - Student = 3
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.RegisterAsync(registerDto);
                _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);

                return Ok(new
                {
                    success = true,
                    message = "User registered successfully",
                    data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Login and receive JWT access token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.LoginAsync(loginDto);
                _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                _logger.LogInformation("Token refreshed successfully for user: {UserId}", response.UserId);

                return Ok(new
                {
                    success = true,
                    message = "Token refreshed successfully",
                    data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during token refresh"
                });
            }
        }

        /// <summary>
        /// Revoke (logout) a refresh token
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RevokeTokenAsync(refreshTokenDto.RefreshToken);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Invalid refresh token" });
                }

                _logger.LogInformation("Token revoked successfully");

                return Ok(new
                {
                    success = true,
                    message = "Token revoked successfully (logged out)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify authentication
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                success = true,
                data = new
                {
                    userId,
                    email,
                    name,
                    role
                }
            });
        }
    }
}