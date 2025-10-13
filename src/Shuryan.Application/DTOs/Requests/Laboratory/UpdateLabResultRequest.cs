using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class UpdateLabResultRequest
    {
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Result value must be between 1-500 characters")]
        public string? ResultValue { get; set; }

        [StringLength(200, ErrorMessage = "Reference range cannot exceed 200 characters")]
        public string? ReferenceRange { get; set; }

        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
        public string? Unit { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
}
