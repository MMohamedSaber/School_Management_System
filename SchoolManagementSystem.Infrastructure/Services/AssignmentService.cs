using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Assignment;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly AppDbContext _context;

        public AssignmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto, int teacherId)
        {
            // Validate class exists and teacher owns it
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId && c.TeacherId == teacherId && c.IsActive);

            if (classEntity == null)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            // Validate due date is not in the past
            if (dto.DueDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException(
                    "Assignment due date cannot be in the past");
            }

            // Create assignment
            var assignment = new Core.Entities.Assignment
            {
                ClassId = dto.ClassId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CreatedByTeacherId = teacherId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(assignment.Id, teacherId);
        }

        public async Task<AssignmentResponseDto> UpdateAsync(int id, UpdateAssignmentDto dto, int teacherId)
        {
            // Get assignment and validate teacher owns it
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .FirstOrDefaultAsync(a => a.Id == id && a.CreatedByTeacherId == teacherId);

            if (assignment == null)
            {
                throw new KeyNotFoundException(
                    "Assignment not found or you don't have access");
            }

            // Validate due date is not in the past
            if (dto.DueDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException(
                    "Assignment due date cannot be in the past");
            }

            // Update assignment
            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(assignment.Id, teacherId);
        }

        public async Task<bool> DeleteAsync(int id, int teacherId)
        {
            // Get assignment and validate teacher owns it
            var assignment = await _context.Assignments
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id && a.CreatedByTeacherId == teacherId);

            if (assignment == null)
            {
                throw new KeyNotFoundException(
                    "Assignment not found or you don't have access");
            }

            // Check if there are submissions
            if (assignment.Submissions.Any())
            {
                throw new InvalidOperationException(
                    "Cannot delete assignment with existing submissions");
            }

            // Delete assignment
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PaginatedResult<AssignmentResponseDto>> GetClassAssignmentsAsync(
            int classId,
            int teacherId,
            int pageNumber,
            int pageSize)
        {
            // Validate class exists and teacher owns it
            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == classId && c.TeacherId == teacherId);

            if (!classExists)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            var query = _context.Assignments
                .Include(a => a.Class)
                .Include(a => a.CreatedByTeacher)
                .Include(a => a.Submissions)
                .Where(a => a.ClassId == classId)
                .AsQueryable();

            // Total count
            var totalCount = await query.CountAsync();

            // Get paginated data
            var assignments = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssignmentResponseDto
                {
                    Id = a.Id,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    CreatedByTeacherId = a.CreatedByTeacherId,
                    CreatedByTeacherName = a.CreatedByTeacher.Name,
                    CreatedDate = a.CreatedDate,
                    TotalSubmissions = a.Submissions.Count,
                    GradedSubmissions = a.Submissions.Count(s => s.Grade != null),
                    PendingSubmissions = a.Submissions.Count(s => s.Grade == null)
                })
                .ToListAsync();

            return new PaginatedResult<AssignmentResponseDto>(
                assignments,
                totalCount,
                pageNumber,
                pageSize);
        }

        public async Task<AssignmentResponseDto> GetByIdAsync(int id, int teacherId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                .Include(a => a.CreatedByTeacher)
                .Include(a => a.Submissions)
                .Where(a => a.Id == id && a.CreatedByTeacherId == teacherId)
                .FirstOrDefaultAsync();

            if (assignment == null)
            {
                throw new KeyNotFoundException(
                    "Assignment not found or you don't have access");
            }

            return new AssignmentResponseDto
            {
                Id = assignment.Id,
                ClassId = assignment.ClassId,
                ClassName = assignment.Class.Name,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                CreatedByTeacherId = assignment.CreatedByTeacherId,
                CreatedByTeacherName = assignment.CreatedByTeacher.Name,
                CreatedDate = assignment.CreatedDate,
                TotalSubmissions = assignment.Submissions.Count,
                GradedSubmissions = assignment.Submissions.Count(s => s.Grade != null),
                PendingSubmissions = assignment.Submissions.Count(s => s.Grade == null)
            };
        }

        public async Task<List<SubmissionResponseDto>> GetAssignmentSubmissionsAsync(
            int assignmentId,
            int teacherId)
        {
            // Validate assignment exists and teacher owns it
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CreatedByTeacherId == teacherId);

            if (assignment == null)
            {
                throw new KeyNotFoundException(
                    "Assignment not found or you don't have access");
            }

            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .Include(s => s.GradedByTeacher)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.Student.Name)
                .Select(s => new SubmissionResponseDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId = s.StudentId,
                    StudentName = s.Student.Name,
                    StudentEmail = s.Student.Email,
                    SubmittedDate = s.SubmittedDate,
                    FileUrl = s.FileUrl,
                    Grade = s.Grade,
                    GradedByTeacherId = s.GradedByTeacherId,
                    GradedByTeacherName = s.GradedByTeacher != null ? s.GradedByTeacher.Name : null,
                    Remarks = s.Remarks,
                    IsGraded = s.Grade != null
                })
                .ToListAsync();

            return submissions;
        }

        public async Task<SubmissionResponseDto> GradeSubmissionAsync(
            int submissionId,
            GradeSubmissionDto dto,
            int teacherId)
        {
            // Get submission with assignment
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                throw new KeyNotFoundException("Submission not found");
            }

            // Validate only assigned teacher can grade
            if (submission.Assignment.Class.TeacherId != teacherId)
            {
                throw new InvalidOperationException(
                    "Only the assigned teacher can grade this submission");
            }

            // Update submission
            submission.Grade = dto.Grade;
            submission.Remarks = dto.Remarks;
            submission.GradedByTeacherId = teacherId;

            await _context.SaveChangesAsync();

            // Return updated submission
            var result = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .Include(s => s.GradedByTeacher)
                .Where(s => s.Id == submissionId)
                .Select(s => new SubmissionResponseDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    StudentId = s.StudentId,
                    StudentName = s.Student.Name,
                    StudentEmail = s.Student.Email,
                    SubmittedDate = s.SubmittedDate,
                    FileUrl = s.FileUrl,
                    Grade = s.Grade,
                    GradedByTeacherId = s.GradedByTeacherId,
                    GradedByTeacherName = s.GradedByTeacher.Name,
                    Remarks = s.Remarks,
                    IsGraded = true
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
