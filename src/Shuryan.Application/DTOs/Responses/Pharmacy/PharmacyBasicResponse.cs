using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Identity;
using System;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyBasicResponse : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public Status PharmacyStatus { get; set; }
        public bool OffersDelivery { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
    }
}
