using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Assignment;
using SchoolManagementSystem.Core.DTOs.Student;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public StudentService(AppDbContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<List<StudentClassDto>> GetEnrolledClassesAsync(int studentId)
        {
            var classes = await _context.StudentClasses
                .Include(sc => sc.Class)
                    .ThenInclude(c => c.Course)
                .Include(sc => sc.Class.Teacher)
                .Where(sc => sc.StudentId == studentId)
                .OrderByDescending(sc => sc.Class.IsActive)
                .ThenByDescending(sc => sc.EnrollmentDate)
                .Select(sc => new StudentClassDto
                {
                    ClassId = sc.ClassId,
                    ClassName = sc.Class.Name,
                    CourseCode = sc.Class.Course.Code,
                    CourseName = sc.Class.Course.Name,
                    Credits = sc.Class.Course.Credits,
                    TeacherName = sc.Class.Teacher.Name,
                    Semester = sc.Class.Semester,
                    StartDate = sc.Class.StartDate,
                    EndDate = sc.Class.EndDate,
                    IsActive = sc.Class.IsActive,
                    EnrollmentDate = sc.EnrollmentDate
                })
                .ToListAsync();

            return classes;
        }

        public async Task<List<StudentAttendanceDto>> GetAttendanceAsync(int studentId, int? classId = null)
        {
            var query = _context.Attendances
                .Include(a => a.Class)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.StudentId == studentId)
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(a => a.ClassId == classId.Value);
            }

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .Select(a => new StudentAttendanceDto
                {
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    MarkedByTeacherName = a.MarkedByTeacher.Name
                })
                .ToListAsync();

            return attendances;
        }

        public async Task<List<StudentGradeDto>> GetGradesAsync(int studentId)
        {
            var grades = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Class)
                        .ThenInclude(c => c.Course)
                .Include(s => s.GradedByTeacher)
                .Where(s => s.StudentId == studentId && s.Grade != null)
                .OrderByDescending(s => s.SubmittedDate)
                .Select(s => new StudentGradeDto
                {
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    ClassName = s.Assignment.Class.Name,
                    CourseCode = s.Assignment.Class.Course.Code,
                    SubmittedDate = s.SubmittedDate,
                    Grade = s.Grade.Value,
                    Remarks = s.Remarks,
                    GradedByTeacherName = s.GradedByTeacher.Name
                })
                .ToListAsync();

            return grades;
        }

        public async Task<List<StudentAssignmentDto>> GetAssignmentsAsync(
            int studentId,
            int? classId = null,
            bool? onlyPending = null)
        {
            // Get student's enrolled classes
            var enrolledClassIds = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.ClassId)
                .ToListAsync();

            var query = _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Course)
                .Include(a => a.Class.Teacher)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .Where(a => enrolledClassIds.Contains(a.ClassId))
                .AsQueryable();

            if (classId.HasValue)
            {
                query = query.Where(a => a.ClassId == classId.Value);
            }

            var assignments = await query
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

            var result = assignments.Select(a =>
            {
                var submission = a.Submissions.FirstOrDefault();
                var isOverdue = a.DueDate < DateTime.UtcNow && submission == null;

                return new StudentAssignmentDto
                {
                    AssignmentId = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    CourseCode = a.Class.Course.Code,
                    TeacherName = a.Class.Teacher.Name,
                    IsSubmitted = submission != null,
                    SubmittedDate = submission?.SubmittedDate,
                    Grade = submission?.Grade,
                    IsOverdue = isOverdue
                };
            }).ToList();

            // Filter by pending if requested
            if (onlyPending.HasValue && onlyPending.Value)
            {
                result = result.Where(a => !a.IsSubmitted).ToList();
            }

            return result;
        }

        public async Task<SubmissionResponseDto> SubmitAssignmentAsync(
            int assignmentId,
            SubmitAssignmentDto dto,
            int studentId)
        {
            // Validate assignment exists
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
            {
                throw new KeyNotFoundException("Assignment not found");
            }

            // Validate student is enrolled in class
            var isEnrolled = await _context.StudentClasses
                .AnyAsync(sc => sc.ClassId == assignment.ClassId && sc.StudentId == studentId);

            if (!isEnrolled)
            {
                throw new InvalidOperationException(
                    "You are not enrolled in this class");
            }

            // Check for duplicate submission
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s =>
                    s.AssignmentId == assignmentId &&
                    s.StudentId == studentId);

            if (existingSubmission != null)
            {
                throw new InvalidOperationException(
                    "You have already submitted this assignment");
            }

            // Create submission
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileUrl = dto.FileUrl,
                SubmittedDate = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Return submission details
            var student = await _context.Users.FindAsync(studentId);

            return new SubmissionResponseDto
            {
                Id = submission.Id,
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                StudentId = studentId,
                StudentName = student.Name,
                StudentEmail = student.Email,
                SubmittedDate = submission.SubmittedDate,
                FileUrl = submission.FileUrl,
                Grade = null,
                IsGraded = false
            };
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(int studentId)
        {
            // Get enrolled classes
            var enrolledClasses = await _context.StudentClasses
                .Include(sc => sc.Class)
                .Where(sc => sc.StudentId == studentId)
                .ToListAsync();

            var enrolledClassIds = enrolledClasses.Select(sc => sc.ClassId).ToList();

            // Get assignments
            var allAssignments = await _context.Assignments
                .Where(a => enrolledClassIds.Contains(a.ClassId))
                .ToListAsync();

            var submissions = await _context.Submissions
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            var submittedAssignmentIds = submissions.Select(s => s.AssignmentId).ToList();

            // Get attendance
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var totalAttendance = attendances.Count;
            var presentCount = attendances.Count(a =>
                a.Status == AttendanceStatus.Present ||
                a.Status == AttendanceStatus.Late);

            var attendancePercentage = totalAttendance > 0
                ? Math.Round((decimal)presentCount / totalAttendance * 100, 2)
                : 0;

            // Calculate average grade
            var gradedSubmissions = submissions.Where(s => s.Grade.HasValue).ToList();
            var averageGrade = gradedSubmissions.Any()
                ? Math.Round(gradedSubmissions.Average(s => s.Grade.Value), 2)
                : (decimal?)null;

            return new StudentDashboardDto
            {
                TotalClasses = enrolledClasses.Count,
                ActiveClasses = enrolledClasses.Count(sc => sc.Class.IsActive),
                TotalAssignments = allAssignments.Count,
                PendingAssignments = allAssignments.Count(a => !submittedAssignmentIds.Contains(a.Id)),
                SubmittedAssignments = submissions.Count,
                GradedAssignments = gradedSubmissions.Count,
                AverageGrade = averageGrade,
                AttendancePercentage = attendancePercentage
            };
        }

        public async Task<SubmissionResponseDto> SubmitAssignmentWithFileAsync(int assignmentId, IFormFile file, int studentId)
        {
            // Validate file
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar" };
            var maxSizeInBytes = 10 * 1024 * 1024; // 10 MB

            if (!_fileUploadService.ValidateFile(file, maxSizeInBytes, allowedExtensions))
            {
                throw new InvalidOperationException(
                    "Invalid file. Allowed: PDF, DOC, DOCX, TXT, ZIP, RAR (Max 10MB)");
            }

            // Validate assignment exists
            var assignment = await _context.Assignments
                .Include(a => a.Class)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
            {
                throw new KeyNotFoundException("Assignment not found");
            }

            // Validate student is enrolled in class
            var isEnrolled = await _context.StudentClasses
                .AnyAsync(sc => sc.ClassId == assignment.ClassId && sc.StudentId == studentId);

            if (!isEnrolled)
            {
                throw new InvalidOperationException(
                    "You are not enrolled in this class");
            }

            // Check for duplicate submission
            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s =>
                    s.AssignmentId == assignmentId &&
                    s.StudentId == studentId);

            if (existingSubmission != null)
            {
                throw new InvalidOperationException(
                    "You have already submitted this assignment");
            }

            // Upload file
            var fileUrl = await _fileUploadService.UploadFileAsync(file, "assignments");

            // Create submission
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileUrl = fileUrl,
                SubmittedDate = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Return submission details
            var student = await _context.Users.FindAsync(studentId);

            return new SubmissionResponseDto
            {
                Id = submission.Id,
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                StudentId = studentId,
                StudentName = student.Name,
                StudentEmail = student.Email,
                SubmittedDate = submission.SubmittedDate,
                FileUrl = submission.FileUrl,
                Grade = null,
                IsGraded = false
            };
        }

        public async Task<(byte[] fileData, string fileName)?> GetSubmissionFileAsync(
        int submissionId,
        int studentId)
        {
            // Get submission
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == submissionId && s.StudentId == studentId);

            if (submission == null || string.IsNullOrEmpty(submission.FileUrl))
                return null;

            // Get file path
            var fileName = Path.GetFileName(submission.FileUrl);
            var folder = Path.GetFileName(Path.GetDirectoryName(submission.FileUrl));

            // Note: You need IWebHostEnvironment in StudentService
            // Update constructor to include it
            var filePath = Path.Combine("wwwroot", "uploads", folder, fileName);

            if (!File.Exists(filePath))
                return null;

            // Read file
            var fileData = await File.ReadAllBytesAsync(filePath);
            return (fileData, fileName);
        }
    }
}


