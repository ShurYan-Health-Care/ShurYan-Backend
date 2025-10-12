using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    public class CreateMedicationRequest
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Brand name must be between 2-200 characters")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(200, MinimumLength = 2, ErrorMessage = "Generic name must be between 2-200 characters")]
        public string? GenericName { get; set; }

        [StringLength(100, ErrorMessage = "Strength cannot exceed 100 characters")]
        public string? Strength { get; set; }

        [Required(ErrorMessage = "Dosage form is required")]
        public DosageForm DosageForm { get; set; }
    }
}
