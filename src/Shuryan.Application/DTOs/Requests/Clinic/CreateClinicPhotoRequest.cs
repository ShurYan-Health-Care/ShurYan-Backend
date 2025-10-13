using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicPhotoRequest
    {
        [Required(ErrorMessage = "Photo URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string PhotoUrl { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }
    }
}
