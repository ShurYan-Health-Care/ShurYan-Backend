using Microsoft.AspNetCore.Http;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class UploadPharmacyDocumentRequest
    {
        public int Type { get; set; }
        public IFormFile Document { get; set; } = null!;
    }
}
