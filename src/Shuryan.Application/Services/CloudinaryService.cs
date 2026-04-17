using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.FileUpload;
using Shuryan.Application.Interfaces;
using Shuryan.Shared.Configurations;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shuryan.Application.Services
{
    public class CloudinaryService : IFileUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
        private const long MaxDocumentSize = 10 * 1024 * 1024; // 10MB
        private const long MaxVideoSize = 50 * 1024 * 1024; // 50MB

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<FileUploadDto> UploadProfileImageAsync(IFormFile file, string userId)
        {
            try
            {
                // Validate file
                ValidateFile(file, _allowedImageExtensions, MaxImageSize, "image");

                // Upload to Cloudinary
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = $"shuryan/profiles/{userId}",
                    Transformation = new Transformation()
                        .Width(500)
                        .Height(500)
                        .Crop("fill")
                        .Gravity("face"),
                    PublicId = $"profile_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Profile image uploaded successfully: {PublicId}", uploadResult.PublicId);

                return new FileUploadDto
                {
                    FileName = uploadResult.PublicId,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FileUploadDto> UploadDocumentAsync(IFormFile file, string userId)
        {
            try
            {
                // Validate file
                ValidateFile(file, _allowedDocumentExtensions, MaxDocumentSize, "document");

                // Upload to Cloudinary
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = $"shuryan/documents/{userId}",
                    PublicId = $"doc_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Document uploaded successfully: {PublicId}", uploadResult.PublicId);

                return new FileUploadDto
                {
                    FileName = uploadResult.PublicId,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FileUploadDto> UploadVideoAsync(IFormFile file, string userId)
        {
            try
            {
                // Validate file
                ValidateFile(file, _allowedVideoExtensions, MaxVideoSize, "video");

                // Upload to Cloudinary
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = $"shuryan/videos/{userId}",
                    PublicId = $"video_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Video uploaded successfully: {PublicId}", uploadResult.PublicId);

                return new FileUploadDto
                {
                    FileName = uploadResult.PublicId,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FileUploadDto> UploadClinicImageAsync(IFormFile file, string doctorId)
        {
            try
            {
                // Validate file
                ValidateFile(file, _allowedImageExtensions, MaxImageSize, "clinic image");

                // Upload to Cloudinary
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = $"shuryan/clinics/{doctorId}",
                    Transformation = new Transformation()
                        .Width(1200)
                        .Height(800)
                        .Crop("fill")
                        .Quality("auto"),
                    PublicId = $"clinic_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Clinic image uploaded successfully for doctor {DoctorId}: {PublicId}", doctorId, uploadResult.PublicId);

                return new FileUploadDto
                {
                    FileName = uploadResult.PublicId,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading clinic image for doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                    return false;

                // Extract public ID from URL
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split('/');
                var publicIdWithExtension = string.Join("/", segments.Skip(segments.Length - 3));
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);

                // Delete from Cloudinary
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok")
                {
                    _logger.LogInformation("File deleted successfully: {PublicId}", publicId);
                    return true;
                }

                _logger.LogWarning("File deletion failed: {Result}", result.Result);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                throw;
            }
        }

        private void ValidateFile(IFormFile file, string[] allowedExtensions, long maxSize, string fileType)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException($"{fileType} file is required");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid {fileType} file type. Allowed types: {string.Join(", ", allowedExtensions)}");

            if (file.Length > maxSize)
                throw new ArgumentException($"{fileType} file size exceeds the maximum allowed size of {maxSize / 1024 / 1024}MB");
        }
    }
}
