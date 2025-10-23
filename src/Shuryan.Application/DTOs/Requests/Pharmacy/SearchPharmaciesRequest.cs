using Shuryan.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class SearchPharmaciesRequest
    {
        public string? SearchTerm { get; set; }

        public Governorate? Governorate { get; set; }

        public bool? OffersDelivery { get; set; }

        public double? MinRating { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
