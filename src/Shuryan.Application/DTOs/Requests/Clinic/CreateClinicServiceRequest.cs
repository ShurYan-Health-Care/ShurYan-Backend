using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicServiceRequest
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Service name must be between 3-200 characters")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}
