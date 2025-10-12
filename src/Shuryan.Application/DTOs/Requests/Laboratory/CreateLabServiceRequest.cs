using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabServiceRequest
    {
        [Required(ErrorMessage = "Lab test ID is required")]
        public Guid LabTestId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000")]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [StringLength(500, ErrorMessage = "Lab specific notes cannot exceed 500 characters")]
        public string? LabSpecificNotes { get; set; }
    }
}
