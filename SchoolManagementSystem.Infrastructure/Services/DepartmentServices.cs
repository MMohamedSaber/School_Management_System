using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null)
        {
            var query = _context.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Courses)
                .Where(d => d.IsActive)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(d =>
                    d.Name.Contains(searchTerm) ||
                    d.Description.Contains(searchTerm));
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Pagination
            var departments = await query
                .OrderBy(d => d.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    HeadOfDepartmentId = d.HeadOfDepartmentId,
                    HeadOfDepartmentName = d.HeadOfDepartment != null ? d.HeadOfDepartment.Name : null,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate,
                    IsActive = d.IsActive,
                    CoursesCount = d.Courses.Count(c => c.IsActive)
                })
                .ToListAsync();

            return new PaginatedResult<DepartmentResponseDto>(
                departments,
                totalCount,
                pageNumber,
                pageSize);
        }

        public async Task<DepartmentResponseDto> GetByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Courses)
                .Where(d => d.Id == id && d.IsActive)
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    HeadOfDepartmentId = d.HeadOfDepartmentId,
                    HeadOfDepartmentName = d.HeadOfDepartment != null ? d.HeadOfDepartment.Name : null,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate,
                    IsActive = d.IsActive,
                    CoursesCount = d.Courses.Count(c => c.IsActive)
                })
                .FirstOrDefaultAsync();

            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID {id} not found");
            }

            return department;
        }

        public async Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto)
        {
            // Validate unique department name
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name.ToLower() == dto.Name.ToLower() && d.IsActive);

            if (existingDepartment != null)
            {
                throw new InvalidOperationException($"Department with name '{dto.Name}' already exists");
            }

            // Validate HeadOfDepartmentId is a teacher (if provided)
            if (dto.HeadOfDepartmentId.HasValue)
            {
                var teacher = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == dto.HeadOfDepartmentId.Value &&
                        u.Role == UserRole.Teacher &&
                        u.IsActive);

                if (teacher == null)
                {
                    throw new InvalidOperationException(
                        $"User with ID {dto.HeadOfDepartmentId.Value} is not a valid teacher");
                }
            }

            // Create department
            var department = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                HeadOfDepartmentId = dto.HeadOfDepartmentId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(department.Id);
        }

        public async Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID {id} not found");
            }

            // Validate unique department name (excluding current department)
            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d =>
                    d.Name.ToLower() == dto.Name.ToLower() &&
                    d.Id != id &&
                    d.IsActive);

            if (existingDepartment != null)
            {
                throw new InvalidOperationException($"Department with name '{dto.Name}' already exists");
            }

            // Validate HeadOfDepartmentId is a teacher (if provided)
            if (dto.HeadOfDepartmentId.HasValue)
            {
                var teacher = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == dto.HeadOfDepartmentId.Value &&
                        u.Role == UserRole.Teacher &&
                        u.IsActive);

                if (teacher == null)
                {
                    throw new InvalidOperationException(
                        $"User with ID {dto.HeadOfDepartmentId.Value} is not a valid teacher");
                }
            }

            // Update department
            department.Name = dto.Name;
            department.Description = dto.Description;
            department.HeadOfDepartmentId = dto.HeadOfDepartmentId;
            department.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(department.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Courses)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID {id} not found");
            }

            // Check if department has active courses
            var hasActiveCourses = department.Courses.Any(c => c.IsActive);
            if (hasActiveCourses)
            {
                throw new InvalidOperationException(
                    "Cannot delete department with active courses. Please deactivate or reassign courses first.");
            }

            // Soft delete
            department.IsActive = false;
            department.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}