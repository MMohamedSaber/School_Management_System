using Microsoft.AspNetCore.Http;

namespace SchoolManagementSystem.Core.DTOs
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
    }
}
