namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// تقييم معمل
        /// </summary>
        public class LaboratoryReviewResponse
        {
                public Guid Id { get; set; }
                public string PatientName { get; set; } = string.Empty;
                public string? PatientProfileImage { get; set; }

                // Ratings
                public int OverallSatisfaction { get; set; }
                public int ResultAccuracy { get; set; }
                public int DeliverySpeed { get; set; }
                public int ServiceQuality { get; set; }
                public int ValueForMoney { get; set; }
                public double AverageRating { get; set; }

                public DateTime CreatedAt { get; set; }
                public bool IsEdited { get; set; }
        }
}
