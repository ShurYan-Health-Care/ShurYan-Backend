using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class ClinicImagesListResponse
    {
        public List<ClinicImageResponse> Images { get; set; } = new List<ClinicImageResponse>();
    }
}
