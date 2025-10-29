using Microsoft.AspNetCore.Http;
using Shuryan.Application.DTOs.Common.FileUpload;
using System.Threading.Tasks;

namespace Shuryan.Application.Interfaces
{
    public interface IFileUploadService
    {
        /// </summary>
        Task<FileUploadDto> UploadProfileImageAsync(IFormFile file, string userId);

        Task<FileUploadDto> UploadDocumentAsync(IFormFile file, string userId);

        Task<FileUploadDto> UploadVideoAsync(IFormFile file, string userId);

        Task<FileUploadDto> UploadClinicImageAsync(IFormFile file, string doctorId);

        Task<bool> DeleteFileAsync(string fileUrl);
    }
}
