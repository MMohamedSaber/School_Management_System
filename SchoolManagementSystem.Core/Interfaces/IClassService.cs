using SchoolManagementSystem.Core.DTOs.Class;
using SchoolManagementSystem.Core.DTOs.Department;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IClassService
    {
        Task<PaginatedResult<ClassResponseDto>> GetTeacherClassesAsync(int teacherId, int pageNumber, int pageSize, string searchTerm = null);
        Task<ClassResponseDto> GetByIdAsync(int id, int teacherId);
        Task<ClassResponseDto> CreateAsync(CreateClassDto dto, int teacherId);
        Task<ClassResponseDto> UpdateAsync(int id, UpdateClassDto dto, int teacherId);
        Task<ClassResponseDto> DeactivateAsync(int id, int teacherId);
        Task<StudentEnrollmentResponseDto> EnrollStudentAsync(int classId, EnrollStudentDto dto, int teacherId);
        Task<List<StudentEnrollmentResponseDto>> GetClassStudentsAsync(int classId, int teacherId);
    }
}
