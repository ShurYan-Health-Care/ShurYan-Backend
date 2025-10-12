using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LaboratoryDto : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public Status LaboratoryStatus { get; set; }
        public bool OffersHomeSampleCollection { get; set; }
        public decimal? HomeSampleCollectionFee { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifierId { get; set; }
        public AddressDto? Address { get; set; }
        public IEnumerable<LabWorkingHoursDto> WorkingHours { get; set; } = new List<LabWorkingHoursDto>();
        public IEnumerable<LabServiceDto> LabServices { get; set; } = new List<LabServiceDto>();
        public double? AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }
}
