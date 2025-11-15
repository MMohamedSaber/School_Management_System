
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        int? ValidateToken(string token);
    }
}
