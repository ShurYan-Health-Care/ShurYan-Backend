using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Common.Address;
using Shuryan.Core.Enums.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyResponse : BaseAuditableDto
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
        public AddressResponse? Address { get; set; }
        public IEnumerable<PharmacyWorkingHoursResponse> WorkingHours { get; set; } = new List<PharmacyWorkingHoursResponse>();
        public double? AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
    }
}

