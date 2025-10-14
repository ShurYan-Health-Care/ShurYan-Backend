using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Identity;
using System;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LaboratoryBasicResponse : BaseAuditableDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public Status LaboratoryStatus { get; set; }
        public bool OffersHomeSampleCollection { get; set; }
        public decimal? HomeSampleCollectionFee { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
    }
}
