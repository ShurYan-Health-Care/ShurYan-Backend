using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Prescription
{
    public class UpdateMedicationDto
    {
        public string? BrandName { get; set; }
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public DosageForm? DosageForm { get; set; }
    }
}
