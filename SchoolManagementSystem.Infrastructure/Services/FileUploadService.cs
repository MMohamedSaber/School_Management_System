using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {

        private readonly IHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                // Create uploads directory if not exists
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                var fileUrl = $"/uploads/{folder}/{uniqueFileName}";
                _logger.LogInformation("File uploaded successfully: {FileUrl}", fileUrl);

                return fileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw new InvalidOperationException("Failed to upload file");
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return false;

                // Convert URL to physical path
                var fileName = Path.GetFileName(fileUrl);
                var folder = Path.GetFileName(Path.GetDirectoryName(fileUrl));
                var filePath = Path.Combine(_environment.ContentRootPath, "uploads", folder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                return false;
            }
        }

        public bool ValidateFile(IFormFile file, long maxSizeInBytes, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > maxSizeInBytes)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return false;

            return true;
        }
    }
}
