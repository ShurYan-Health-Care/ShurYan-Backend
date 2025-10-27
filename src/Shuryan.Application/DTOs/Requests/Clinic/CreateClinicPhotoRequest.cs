using Microsoft.AspNetCore.Http;
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
        [Required(ErrorMessage = "Photo file is required")]
        public IFormFile PhotoFile { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Caption cannot exceed 500 characters")]
        public string? Caption { get; set; }
    }
}
