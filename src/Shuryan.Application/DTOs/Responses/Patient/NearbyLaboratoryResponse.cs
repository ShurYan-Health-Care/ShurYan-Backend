namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// معمل قريب من المريض
        /// </summary>
        public class NearbyLaboratoryResponse
        {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string? Description { get; set; }
                public string? ProfileImageUrl { get; set; }
                public string PhoneNumber { get; set; } = string.Empty;
                public string? WhatsAppNumber { get; set; }

                // Location
                public double DistanceInKm { get; set; }
                public string? Address { get; set; }
                public double? Latitude { get; set; }
                public double? Longitude { get; set; }

                // Services
                public bool OffersHomeSampleCollection { get; set; }
                public decimal? HomeSampleCollectionFee { get; set; }
                public int AvailableServicesCount { get; set; }

                // Rating
                public double? AverageRating { get; set; }
                public int TotalReviewsCount { get; set; }

                // Working Hours
                public bool IsOpenNow { get; set; }
                public string? TodayWorkingHours { get; set; }
        }
}
