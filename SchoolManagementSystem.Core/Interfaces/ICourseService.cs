using SchoolManagementSystem.Core.DTOs.Course;
using SchoolManagementSystem.Core.DTOs.Department;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface ICourseService
    {
        Task<PaginatedResult<CourseResponseDto>> GetAllAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            int? departmentId = null,
            int? minCredits = null,
            int? maxCredits = null);

        Task<CourseResponseDto> GetByIdAsync(int id);
        Task<CourseResponseDto> CreateAsync(CreateCourseDto dto);
        Task<CourseResponseDto> UpdateAsync(int id, UpdateCourseDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
