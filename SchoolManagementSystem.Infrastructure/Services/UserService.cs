using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.DTOs.User;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;

        public UserService(AppDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<PaginatedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter)
        {
            var query = _context.Users.AsQueryable();

            // Search by name or email
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(search)
                || u.Email.ToLower().Contains(search));
            }

            // Filter by role
            if (filter.Role.HasValue && Enum.IsDefined(typeof(UserRole), filter.Role.Value))
            {
                var role = (UserRole)filter.Role.Value;
                query = query.Where(u => u.Role == role);
            }

            // Filter by IsActive
            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated data
            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return new PaginatedResult<UserResponseDto>(
                users,
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }

        public async Task<UserResponseDto> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            return user;
        }

        public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto)
        {
            // Get existing user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), dto.Role))
            {
                throw new InvalidOperationException("Invalid role");
            }

            // Check if email is unique (excluding current user)
            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email.ToLower() == dto.Email.ToLower() &&
                    u.Id != id);

            if (emailExists)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already in use");
            }

            // Validate role change restrictions
            // Prevent changing role if user has dependencies
            if (user.Role != (UserRole)dto.Role)
            {
                await ValidateRoleChange(user, (UserRole)dto.Role);
            }

            // Update user
            user.Name = dto.Name;
            user.Email = dto.Email.ToLower();
            user.Role = (UserRole)dto.Role;
            user.UpdatedDate = DateTime.UtcNow;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.Password = _passwordService.HashPassword(dto.NewPassword);
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(user.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Prevent deleting yourself (the admin making the request should check this in controller)
            // But we add additional validation here

            // Check dependencies before soft delete
            await ValidateUserDeletion(user);

            // Soft delete
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["TotalUsers"] = await _context.Users.CountAsync(u => u.IsActive),
                ["TotalAdmins"] = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive),
                ["TotalTeachers"] = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher && u.IsActive),
                ["TotalStudents"] = await _context.Users.CountAsync(u => u.Role == UserRole.Student && u.IsActive),
                ["InactiveUsers"] = await _context.Users.CountAsync(u => !u.IsActive)
            };

            return stats;
        }

        // Private helper methods
        private async Task ValidateRoleChange(User user, UserRole newRole)
        {
            // If changing from Teacher to Student/Admin, check if they have dependencies
            if (user.Role == UserRole.Teacher)
            {
                var hasClasses = await _context.Classes.AnyAsync(c => c.TeacherId == user.Id && c.IsActive);
                if (hasClasses)
                {
                    throw new InvalidOperationException(
                        "Cannot change role: User is currently assigned as teacher to active classes");
                }

                var isHeadOfDepartment = await _context.Departments
                    .AnyAsync(d => d.HeadOfDepartmentId == user.Id && d.IsActive);
                if (isHeadOfDepartment)
                {
                    throw new InvalidOperationException(
                        "Cannot change role: User is currently head of department");
                }
            }

            // If changing from Student, check enrollments
            if (user.Role == UserRole.Student)
            {
                var hasEnrollments = await _context.StudentClasses.AnyAsync(sc => sc.StudentId == user.Id);
                if (hasEnrollments)
                {
                    throw new InvalidOperationException(
                        "Cannot change role: User is currently enrolled in classes");
                }
            }
        }

        private async Task ValidateUserDeletion(User user)
        {
            // Check if user is head of department
            if (user.Role == UserRole.Teacher)
            {
                var isHeadOfDepartment = await _context.Departments
                    .AnyAsync(d => d.HeadOfDepartmentId == user.Id && d.IsActive);
                if (isHeadOfDepartment)
                {
                    throw new InvalidOperationException(
                        "Cannot delete user: User is head of department. Please reassign first.");
                }

                var hasActiveClasses = await _context.Classes
                    .AnyAsync(c => c.TeacherId == user.Id && c.IsActive);
                if (hasActiveClasses)
                {
                    throw new InvalidOperationException(
                        "Cannot delete user: User has active classes. Please reassign first.");
                }
            }

            // Check if student has active enrollments
            if (user.Role == UserRole.Student)
            {
                var hasActiveEnrollments = await _context.StudentClasses
                    .AnyAsync(sc => sc.StudentId == user.Id);
                if (hasActiveEnrollments)
                {
                    throw new InvalidOperationException(
                        "Cannot delete user: User has active class enrollments.");
                }
            }
        }
    }
}
