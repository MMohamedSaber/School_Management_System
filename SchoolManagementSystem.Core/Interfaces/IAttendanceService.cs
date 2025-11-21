using SchoolManagementSystem.Core.DTOs.Attendance;
using SchoolManagementSystem.Core.DTOs.Department;


namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceResponseDto> MarkAttendanceAsync(MarkAttendanceDto dto, int teacherId);
        Task<List<AttendanceResponseDto>> BulkMarkAttendanceAsync(BulkMarkAttendanceDto dto, int teacherId);
        Task<PaginatedResult<AttendanceResponseDto>> GetClassAttendanceAsync(AttendanceFilterDto filter, int teacherId);
        Task<AttendanceSummaryDto> GetClassAttendanceSummaryAsync(int classId, int teacherId);
        Task<List<AttendanceResponseDto>> GetStudentAttendanceAsync(int classId, int studentId, int teacherId);
    }
}
