
using SchoolManagementSystem.Core.DTOs.Assignment;
using SchoolManagementSystem.Core.DTOs.Department;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IAssignmentService
    {
        Task<AssignmentResponseDto> CreateAsync(CreateAssignmentDto dto, int teacherId);
        Task<AssignmentResponseDto> UpdateAsync(int id, UpdateAssignmentDto dto, int teacherId);
        Task<bool> DeleteAsync(int id, int teacherId);
        Task<PaginatedResult<AssignmentResponseDto>> GetClassAssignmentsAsync(int classId, int teacherId, int pageNumber, int pageSize);
        Task<AssignmentResponseDto> GetByIdAsync(int id, int teacherId);
        Task<List<SubmissionResponseDto>> GetAssignmentSubmissionsAsync(int assignmentId, int teacherId);
        Task<SubmissionResponseDto> GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto, int teacherId);
    }
}
