using Microsoft.AspNetCore.Http;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class UploadLaboratoryDocumentRequest
    {
        public int Type { get; set; }
        public IFormFile Document { get; set; } = null!;
    }
}
