using Microsoft.AspNetCore.Http;
using SchoolManagementSystem.Core.DTOs.Assignment;
using SchoolManagementSystem.Core.DTOs.Student;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IStudentService
    {
        Task<List<StudentClassDto>> GetEnrolledClassesAsync(int studentId);
        Task<List<StudentAttendanceDto>> GetAttendanceAsync(int studentId, int? classId = null);
        Task<List<StudentGradeDto>> GetGradesAsync(int studentId);
        Task<List<StudentAssignmentDto>> GetAssignmentsAsync(int studentId, int? classId = null, bool? onlyPending = null);
        Task<SubmissionResponseDto> SubmitAssignmentAsync(int assignmentId, SubmitAssignmentDto dto, int studentId);
        Task<StudentDashboardDto> GetDashboardAsync(int studentId);
        Task<SubmissionResponseDto> SubmitAssignmentWithFileAsync(
            int assignmentId,
            IFormFile file,
            int studentId);
        Task<(byte[] fileData, string fileName)?> GetSubmissionFileAsync(int submissionId, int studentId);
    }
}