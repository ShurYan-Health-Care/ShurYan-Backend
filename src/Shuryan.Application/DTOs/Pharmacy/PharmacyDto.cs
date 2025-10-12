using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class PharmacyDto : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public Status PharmacyStatus { get; set; }
        public bool OffersDelivery { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifierId { get; set; }
        public AddressDto? Address { get; set; }
        public IEnumerable<PharmacyWorkingHoursDto> WorkingHours { get; set; } = new List<PharmacyWorkingHoursDto>();
        public double? AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }
}
