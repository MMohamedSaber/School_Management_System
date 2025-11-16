using SchoolManagementSystem.Core.DTOs.Department;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IDepartmentService
    {
        Task<PaginatedResult<DepartmentResponseDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<DepartmentResponseDto> GetByIdAsync(int id);
        Task<DepartmentResponseDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
