using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabTestRequest
    {
        [Required(ErrorMessage = "Test name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Test name must be between 3-200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Test code is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Test code must be between 2-50 characters")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Test category is required")]
        public LabTestCategory Category { get; set; }

        [StringLength(500, ErrorMessage = "Special instructions cannot exceed 500 characters")]
        public string? SpecialInstructions { get; set; }
    }
}
