using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.DTOs.Attendance;
using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.Entities;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceResponseDto> MarkAttendanceAsync(MarkAttendanceDto dto, int teacherId)
        {
            // Validate class exists and teacher is assigned
            var classEntity = await _context.Classes
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId && c.TeacherId == teacherId && c.IsActive);

            if (classEntity == null)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            // Validate student is enrolled in class
            var enrollment = await _context.StudentClasses
                .Include(sc => sc.Student)
                .FirstOrDefaultAsync(sc =>
                    sc.ClassId == dto.ClassId &&
                    sc.StudentId == dto.StudentId);

            if (enrollment == null)
            {
                throw new InvalidOperationException(
                    "Student is not enrolled in this class");
            }

            // Validate status
            if (!Enum.IsDefined(typeof(AttendanceStatus), dto.Status))
            {
                throw new InvalidOperationException("Invalid attendance status");
            }

            // Check if attendance already marked for this date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.ClassId == dto.ClassId &&
                    a.StudentId == dto.StudentId &&
                    a.Date.Date == dto.Date.Date);

            if (existingAttendance != null)
            {
                // Update existing attendance
                existingAttendance.Status = (AttendanceStatus)dto.Status;
                existingAttendance.MarkedByTeacherId = teacherId;
                existingAttendance.CreatedDate = DateTime.UtcNow;
            }
            else
            {
                // Create new attendance
                var attendance = new Core.Entities.Attendance
                {
                    ClassId = dto.ClassId,
                    StudentId = dto.StudentId,
                    Date = dto.Date.Date, // Store only date part
                    Status = (AttendanceStatus)dto.Status,
                    MarkedByTeacherId = teacherId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();

            // Return the attendance record
            var result = await _context.Attendances
                .Include(a => a.Class)
                .Include(a => a.Student)
                .Include(a => a.MarkedByTeacher)
                .Where(a =>
                    a.ClassId == dto.ClassId &&
                    a.StudentId == dto.StudentId &&
                    a.Date.Date == dto.Date.Date)
                .Select(a => new AttendanceResponseDto
                {
                    Id = a.Id,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    MarkedByTeacherName = a.MarkedByTeacher.Name,
                    CreatedDate = a.CreatedDate
                })
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<AttendanceResponseDto>> BulkMarkAttendanceAsync(
            BulkMarkAttendanceDto dto,
            int teacherId)
        {
            // Validate class exists and teacher is assigned
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId && c.TeacherId == teacherId && c.IsActive);

            if (classEntity == null)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            // Get all enrolled students in class
            var enrolledStudentIds = await _context.StudentClasses
                .Where(sc => sc.ClassId == dto.ClassId)
                .Select(sc => sc.StudentId)
                .ToListAsync();

            var results = new List<AttendanceResponseDto>();

            foreach (var studentAttendance in dto.Attendances)
            {
                // Validate student is enrolled
                if (!enrolledStudentIds.Contains(studentAttendance.StudentId))
                {
                    throw new InvalidOperationException(
                        $"Student {studentAttendance.StudentId} is not enrolled in this class");
                }

                // Validate status
                if (!Enum.IsDefined(typeof(AttendanceStatus), studentAttendance.Status))
                {
                    throw new InvalidOperationException("Invalid attendance status");
                }

                // Check if attendance already exists
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a =>
                        a.ClassId == dto.ClassId &&
                        a.StudentId == studentAttendance.StudentId &&
                        a.Date.Date == dto.Date.Date);

                if (existingAttendance != null)
                {
                    // Update existing
                    existingAttendance.Status = (AttendanceStatus)studentAttendance.Status;
                    existingAttendance.MarkedByTeacherId = teacherId;
                    existingAttendance.CreatedDate = DateTime.UtcNow;
                }
                else
                {
                    // Create new
                    var attendance = new Core.Entities.Attendance
                    {
                        ClassId = dto.ClassId,
                        StudentId = studentAttendance.StudentId,
                        Date = dto.Date.Date,
                        Status = (AttendanceStatus)studentAttendance.Status,
                        MarkedByTeacherId = teacherId,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Attendances.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();

            // Get all marked attendances for this date
            results = await _context.Attendances
                .Include(a => a.Class)
                .Include(a => a.Student)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.ClassId == dto.ClassId && a.Date.Date == dto.Date.Date)
                .Select(a => new AttendanceResponseDto
                {
                    Id = a.Id,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    MarkedByTeacherName = a.MarkedByTeacher.Name,
                    CreatedDate = a.CreatedDate
                })
                .OrderBy(a => a.StudentName)
                .ToListAsync();

            return results;
        }

        public async Task<PaginatedResult<AttendanceResponseDto>> GetClassAttendanceAsync(
            AttendanceFilterDto filter,
            int teacherId)
        {
            // Validate class exists and teacher is assigned
            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == filter.ClassId && c.TeacherId == teacherId);

            if (!classExists)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            var query = _context.Attendances
                .Include(a => a.Class)
                .Include(a => a.Student)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.ClassId == filter.ClassId)
                .AsQueryable();

            // Filter by student
            if (filter.StudentId.HasValue)
            {
                query = query.Where(a => a.StudentId == filter.StudentId.Value);
            }

            // Filter by date range
            if (filter.FromDate.HasValue)
            {
                query = query.Where(a => a.Date.Date >= filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(a => a.Date.Date <= filter.ToDate.Value.Date);
            }

            // Filter by status
            if (filter.Status.HasValue && Enum.IsDefined(typeof(AttendanceStatus), filter.Status.Value))
            {
                var status = (AttendanceStatus)filter.Status.Value;
                query = query.Where(a => a.Status == status);
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Get paginated data
            var attendances = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Student.Name)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AttendanceResponseDto
                {
                    Id = a.Id,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    MarkedByTeacherName = a.MarkedByTeacher.Name,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();

            return new PaginatedResult<AttendanceResponseDto>(
                attendances,
                totalCount,
                filter.PageNumber,
                filter.PageSize);
        }

        public async Task<AttendanceSummaryDto> GetClassAttendanceSummaryAsync(int classId, int teacherId)
        {
            // Validate class exists and teacher is assigned
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == teacherId);

            if (classEntity == null)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            var totalStudents = await _context.StudentClasses
                .CountAsync(sc => sc.ClassId == classId);

            var attendances = await _context.Attendances
                .Where(a => a.ClassId == classId)
                .ToListAsync();

            var totalSessions = attendances
                .Select(a => a.Date.Date)
                .Distinct()
                .Count();

            var totalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present);
            var totalAbsent = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var totalLate = attendances.Count(a => a.Status == AttendanceStatus.Late);

            var totalRecords = attendances.Count;
            var attendancePercentage = totalRecords > 0
                ? Math.Round((decimal)(totalPresent + totalLate) / totalRecords * 100, 2)
                : 0;

            return new AttendanceSummaryDto
            {
                ClassId = classId,
                ClassName = classEntity.Name,
                TotalStudents = totalStudents,
                TotalSessions = totalSessions,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                TotalLate = totalLate,
                AttendancePercentage = attendancePercentage
            };
        }

        public async Task<List<AttendanceResponseDto>> GetStudentAttendanceAsync(
            int classId,
            int studentId,
            int teacherId)
        {
            // Validate class exists and teacher is assigned
            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == classId && c.TeacherId == teacherId);

            if (!classExists)
            {
                throw new InvalidOperationException(
                    "Class not found or you are not the assigned teacher");
            }

            // Validate student is enrolled
            var enrollmentExists = await _context.StudentClasses
                .AnyAsync(sc => sc.ClassId == classId && sc.StudentId == studentId);

            if (!enrollmentExists)
            {
                throw new InvalidOperationException(
                    "Student is not enrolled in this class");
            }

            var attendances = await _context.Attendances
                .Include(a => a.Class)
                .Include(a => a.Student)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.ClassId == classId && a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceResponseDto
                {
                    Id = a.Id,
                    ClassId = a.ClassId,
                    ClassName = a.Class.Name,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    MarkedByTeacherName = a.MarkedByTeacher.Name,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();

            return attendances;
        }
    }
}
