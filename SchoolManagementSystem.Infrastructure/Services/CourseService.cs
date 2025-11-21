using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Course;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;

        public CourseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<CourseResponseDto>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            int? departmentId = null,
            int? minCredits = null,
            int? maxCredits = null)
        {
            var query = _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Classes)
                .Where(c => c.IsActive)
                .AsQueryable();

            // Search by name, code, or description
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Code.ToLower().Contains(search) ||
                    c.Description != null && c.Description.ToLower().Contains(search));
            }

            // Filter by department
            if (departmentId.HasValue)
            {
                query = query.Where(c => c.DepartmentId == departmentId.Value);
            }

            // Filter by credits range
            if (minCredits.HasValue)
            {
                query = query.Where(c => c.Credits >= minCredits.Value);
            }

            if (maxCredits.HasValue)
            {
                query = query.Where(c => c.Credits <= maxCredits.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated data
            var courses = await query
                .OrderBy(c => c.Department.Name)
                .ThenBy(c => c.Code)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.Department.Name,
                    Credits = c.Credits,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,
                    IsActive = c.IsActive,
                    ClassesCount = c.Classes.Count(cl => cl.IsActive)
                })
                .ToListAsync();

            return new PaginatedResult<CourseResponseDto>(
                courses,
                totalCount,
                pageNumber,
                pageSize);
        }

        public async Task<CourseResponseDto> GetByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Classes)
                .Where(c => c.Id == id && c.IsActive)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found");
            }

            return new CourseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                Code = course.Code,
                Description = course.Description,
                DepartmentId = course.DepartmentId,
                DepartmentName = course.Department.Name,
                Credits = course.Credits,
                CreatedDate = course.CreatedDate,
                UpdatedDate = course.UpdatedDate,
                IsActive = course.IsActive,
                ClassesCount = course.Classes.Count(cl => cl.IsActive)
            };
        }

        public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto)
        {
            // Validate department exists and is active
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == dto.DepartmentId && d.IsActive);

            if (!departmentExists)
            {
                throw new InvalidOperationException($"Department with ID {dto.DepartmentId} not found");
            }

            // Validate unique course code per department
            var codeExists = await _context.Courses
                .AnyAsync(c =>
                    c.Code.ToLower() == dto.Code.ToLower() &&
                    c.DepartmentId == dto.DepartmentId &&
                    c.IsActive);

            if (codeExists)
            {
                throw new InvalidOperationException(
                    $"Course with code '{dto.Code}' already exists in this department");
            }

            // Create new course
            var course = new Course
            {
                Name = dto.Name,
                Code = dto.Code.ToUpper(),
                Description = dto.Description,
                DepartmentId = dto.DepartmentId,
                Credits = dto.Credits,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(course.Id);
        }

        public async Task<CourseResponseDto> UpdateAsync(int id, UpdateCourseDto dto)
        {
            // Get existing course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found");
            }

            // Validate department exists and is active
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.Id == dto.DepartmentId && d.IsActive);

            if (!departmentExists)
            {
                throw new InvalidOperationException($"Department with ID {dto.DepartmentId} not found");
            }

            // Validate unique course code per department (excluding current course)
            var codeExists = await _context.Courses
                .AnyAsync(c =>
                    c.Code.ToLower() == dto.Code.ToLower() &&
                    c.DepartmentId == dto.DepartmentId &&
                    c.Id != id &&
                    c.IsActive);

            if (codeExists)
            {
                throw new InvalidOperationException(
                    $"Course with code '{dto.Code}' already exists in this department");
            }

            // Update course
            course.Name = dto.Name;
            course.Code = dto.Code.ToUpper();
            course.Description = dto.Description;
            course.DepartmentId = dto.DepartmentId;
            course.Credits = dto.Credits;
            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(course.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Get course with related classes
            var course = await _context.Courses
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {id} not found");
            }

            // Check if course has active classes
            var hasActiveClasses = course.Classes.Any(cl => cl.IsActive);
            if (hasActiveClasses)
            {
                throw new InvalidOperationException(
                    "Cannot delete course with active classes. Please deactivate classes first.");
            }

            // Soft delete
            course.IsActive = false;
            course.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
