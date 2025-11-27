namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// تفاصيل معمل كاملة للمريض
        /// </summary>
        public class LaboratoryDetailResponse
        {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string? Description { get; set; }
                public string? ProfileImageUrl { get; set; }
                public string PhoneNumber { get; set; } = string.Empty;
                public string? WhatsAppNumber { get; set; }
                public string? Website { get; set; }

                // Location
                public string? Address { get; set; }
                public string? City { get; set; }
                public string? Area { get; set; }
                public double? Latitude { get; set; }
                public double? Longitude { get; set; }
                public double? DistanceInKm { get; set; }

                // Services
                public bool OffersHomeSampleCollection { get; set; }
                public decimal? HomeSampleCollectionFee { get; set; }

                // Rating
                public double? AverageRating { get; set; }
                public int TotalReviewsCount { get; set; }
                public LaboratoryRatingBreakdown? RatingBreakdown { get; set; }

                // Working Hours
                public bool IsOpenNow { get; set; }
                public List<LaboratoryWorkingHoursResponse> WorkingHours { get; set; } = new();
        }

        public class LaboratoryRatingBreakdown
        {
                public double OverallSatisfaction { get; set; }
                public double ResultAccuracy { get; set; }
                public double DeliverySpeed { get; set; }
                public double ServiceQuality { get; set; }
                public double ValueForMoney { get; set; }
        }

        public class LaboratoryWorkingHoursResponse
        {
                public string DayOfWeek { get; set; } = string.Empty;
                public string DayOfWeekArabic { get; set; } = string.Empty;
                public bool IsClosed { get; set; }
                public string? OpenTime { get; set; }
                public string? CloseTime { get; set; }
        }
}
