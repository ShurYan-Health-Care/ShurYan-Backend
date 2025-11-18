using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Review
{
    public class DoctorReviewStatisticsResponse
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<string, int> RatingDistribution { get; set; } = new Dictionary<string, int>();
    }
}

