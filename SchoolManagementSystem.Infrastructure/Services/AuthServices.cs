using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolManagementSystem.Core.DTOs.Auth;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Core.Settings;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            AppDbContext context,
            IJwtService jwtService,
            IPasswordService passwordService,
            IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), registerDto.Role))
            {
                throw new InvalidOperationException("Invalid role");
            }

            // Create new user
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email.ToLower(),
                Password = _passwordService.HashPassword(registerDto.Password),
                Role = (UserRole)registerDto.Role,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower() && u.IsActive);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(loginDto.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Find refresh token
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Validate token
            if (!storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token is expired or revoked");
            }

            // Check if user is active
            if (!storedToken.User.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive");
            }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(storedToken.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old refresh token
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            // Create new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = storedToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = storedToken.User.Id,
                Name = storedToken.User.Name,
                Email = storedToken.User.Email,
                Role = storedToken.User.Role.ToString(),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return false;
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
