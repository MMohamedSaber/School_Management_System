using SchoolManagementSystem.Core.DTOs.Department;
using SchoolManagementSystem.Core.DTOs.User;


namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter);
        Task<UserResponseDto> GetByIdAsync(int id);
        Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto);
        Task<bool> DeleteAsync(int id);
        Task<Dictionary<string, int>> GetUserStatisticsAsync();
    }
}
