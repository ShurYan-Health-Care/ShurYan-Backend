using System;
using System.Collections.Generic;
using Shuryan.Application.DTOs.Common.Pagination;

namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class SuggestedPartnerResponse
    {
        /// <summary>
        /// الصيدلية المقترحة (null لو مفيش)
        /// </summary>
        public PartnerResponse? Pharmacy { get; set; }

        /// <summary>
        /// المعمل المقترح (null لو مفيش)
        /// </summary>
        public PartnerResponse? Laboratory { get; set; }
    }

    public class PartnerListResponse
    {
        public List<PartnerResponse> Pharmacies { get; set; } = new List<PartnerResponse>();
    }

    public class LaboratoryListResponse
    {
        public List<PartnerResponse> Laboratories { get; set; } = new List<PartnerResponse>();
    }

    public class PartnerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PartnerType { get; set; } = string.Empty; // "pharmacy" or "laboratory"
        public string? OwnerName { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
