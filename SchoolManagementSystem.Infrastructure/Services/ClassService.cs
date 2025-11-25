using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Class;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class ClassService : IClassService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        public ClassService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<PaginatedResult<ClassResponseDto>> GetTeacherClassesAsync(
            int teacherId,
            int pageNumber,
            int pageSize,
            string searchTerm = null)
        {
            var query = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.StudentClasses)
                .Where(c => c.TeacherId == teacherId)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Course.Name.ToLower().Contains(search) ||
                    c.Semester.ToLower().Contains(search));
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Get paginated data
            var classes = await query
                .OrderByDescending(c => c.IsActive)
                .ThenByDescending(c => c.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClassResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CourseId = c.CourseId,
                    CourseName = c.Course.Name,
                    CourseCode = c.Course.Code,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher.Name,
                    Semester = c.Semester,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    EnrolledStudentsCount = c.StudentClasses.Count,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .ToListAsync();

            return new PaginatedResult<ClassResponseDto>(
                classes,
                totalCount,
                pageNumber,
                pageSize);
        }

        public async Task<ClassResponseDto> GetByIdAsync(int id, int teacherId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.StudentClasses)
                .Where(c => c.Id == id && c.TeacherId == teacherId)
                .FirstOrDefaultAsync();

            if (classEntity == null)
            {
                throw new KeyNotFoundException($"Class with ID {id} not found or you don't have access");
            }

            return new ClassResponseDto
            {
                Id = classEntity.Id,
                Name = classEntity.Name,
                CourseId = classEntity.CourseId,
                CourseName = classEntity.Course.Name,
                CourseCode = classEntity.Course.Code,
                TeacherId = classEntity.TeacherId,
                TeacherName = classEntity.Teacher.Name,
                Semester = classEntity.Semester,
                StartDate = classEntity.StartDate,
                EndDate = classEntity.EndDate,
                IsActive = classEntity.IsActive,
                EnrolledStudentsCount = classEntity.StudentClasses.Count,
                CreatedDate = classEntity.CreatedDate,
                UpdatedDate = classEntity.UpdatedDate
            };
        }

        public async Task<ClassResponseDto> CreateAsync(CreateClassDto dto, int teacherId)
        {
            // Validate course exists
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId && c.IsActive);

            if (course == null)
            {
                throw new InvalidOperationException($"Course with ID {dto.CourseId} not found");
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new InvalidOperationException("End date must be after start date");
            }

            // Create class
            var classEntity = new Class
            {
                Name = dto.Name,
                CourseId = dto.CourseId,
                TeacherId = teacherId, // Set current teacher
                Semester = dto.Semester,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(classEntity.Id, teacherId);
        }

        public async Task<ClassResponseDto> UpdateAsync(int id, UpdateClassDto dto, int teacherId)
        {
            // Get class (only if teacher owns it)
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

            if (classEntity == null)
            {
                throw new KeyNotFoundException($"Class with ID {id} not found or you don't have access");
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new InvalidOperationException("End date must be after start date");
            }

            // Update class
            classEntity.Name = dto.Name;
            classEntity.Semester = dto.Semester;
            classEntity.StartDate = dto.StartDate;
            classEntity.EndDate = dto.EndDate;
            classEntity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(classEntity.Id, teacherId);
        }

        public async Task<ClassResponseDto> DeactivateAsync(int id, int teacherId)
        {
            // Get class (only if teacher owns it)
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == id && c.TeacherId == teacherId);

            if (classEntity == null)
            {
                throw new KeyNotFoundException($"Class with ID {id} not found or you don't have access");
            }

            if (!classEntity.IsActive)
            {
                throw new InvalidOperationException("Class is already deactivated");
            }

            // Deactivate
            classEntity.IsActive = false;
            classEntity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(classEntity.Id, teacherId);
        }

        public async Task<StudentEnrollmentResponseDto> EnrollStudentAsync(
            int classId,
            EnrollStudentDto dto,
            int teacherId)
        {
            // Validate class exists and teacher owns it
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId && c.IsActive);

            if (classEntity == null)
            {
                throw new KeyNotFoundException($"Class with ID {classId} not found or you don't have access");
            }

            // Validate student exists and is active
            var student = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.StudentId &&
                    u.Role == UserRole.Student &&
                    u.IsActive);

            if (student == null)
            {
                throw new InvalidOperationException($"Student with ID {dto.StudentId} not found");
            }

            // Check for duplicate enrollment
            var existingEnrollment = await _context.StudentClasses
                .FirstOrDefaultAsync(sc =>
                    sc.StudentId == dto.StudentId &&
                    sc.ClassId == classId);

            if (existingEnrollment != null)
            {
                throw new InvalidOperationException(
                    $"Student is already enrolled in this class");
            }

            // Enroll student
            var enrollment = new StudentClass
            {
                StudentId = dto.StudentId,
                ClassId = classId,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.StudentClasses.Add(enrollment);
            await _context.SaveChangesAsync();

            // Send email notification
            try
            {
                await _emailService.SendClassEnrollmentEmailAsync(
                    student.Email,
                    student.Name,
                    classEntity.Name,
                    classEntity.Course.Name,
                    classEntity.Teacher.Name);
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation if email fails
            }
            return new StudentEnrollmentResponseDto
            {
                Id = enrollment.Id,
                StudentId = student.Id,
                StudentName = student.Name,
                StudentEmail = student.Email,
                ClassId = classId,
                ClassName = classEntity.Name,
                EnrollmentDate = enrollment.EnrollmentDate
            };
        }

        public async Task<List<StudentEnrollmentResponseDto>> GetClassStudentsAsync(int classId, int teacherId)
        {
            // Validate class exists and teacher owns it
            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == classId && c.TeacherId == teacherId);

            if (!classExists)
            {
                throw new KeyNotFoundException($"Class with ID {classId} not found or you don't have access");
            }

            // Get enrolled students
            var students = await _context.StudentClasses
                .Include(sc => sc.Student)
                .Include(sc => sc.Class)
                .Where(sc => sc.ClassId == classId)
                .OrderBy(sc => sc.Student.Name)
                .Select(sc => new StudentEnrollmentResponseDto
                {
                    Id = sc.Id,
                    StudentId = sc.StudentId,
                    StudentName = sc.Student.Name,
                    StudentEmail = sc.Student.Email,
                    ClassId = sc.ClassId,
                    ClassName = sc.Class.Name,
                    EnrollmentDate = sc.EnrollmentDate
                })
                .ToListAsync();

            return students;
        }
    }
}
