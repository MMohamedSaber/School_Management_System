
using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        bool ValidateFile(IFormFile file, long maxSizeInBytes, string[] allowedExtensions);
    }
}
