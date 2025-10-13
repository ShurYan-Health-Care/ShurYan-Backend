using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    public class CreateMedicalHistoryItemRequest
    {
        [Required(ErrorMessage = "Medical history type is required")]
        public MedicalHistoryType Type { get; set; }

        [Required(ErrorMessage = "Medical history text is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Medical history must be between 5-2000 characters")]
        public string Text { get; set; } = string.Empty;
    }
}
