namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyReviewStatisticsResponse
    {
        public Guid PharmacyId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviewsCount { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public double AverageOverallSatisfaction { get; set; }
        public double AverageMedicationAvailability { get; set; }
        public double AverageServiceQuality { get; set; }
        public double AverageDeliverySpeed { get; set; }
        public double AverageValueForMoney { get; set; }
    }
}
