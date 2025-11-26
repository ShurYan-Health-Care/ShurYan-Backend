using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLaboratoryProfileImageRequest
        {
                [Required(ErrorMessage = "صورة البروفايل مطلوبة")]
                public IFormFile ProfileImage { get; set; } = null!;
        }
}
