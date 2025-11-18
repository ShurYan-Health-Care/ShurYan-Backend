using System;

namespace Shuryan.Application.DTOs.Responses.Review
{
    public class DoctorReviewDetailsResponse
    {
        public Guid ReviewId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientProfileImage { get; set; }
        
        // Rating breakdown
        public int OverallSatisfaction { get; set; }
        public int WaitingTime { get; set; }
        public int CommunicationQuality { get; set; }
        public int ClinicCleanliness { get; set; }
        public int ValueForMoney { get; set; }
        public double AverageRating { get; set; }
        
        // Comment
        public string? Comment { get; set; }
        
        // Doctor reply
        public bool HasDoctorReply { get; set; }
        public string? DoctorReply { get; set; }
        public DateTime? DoctorRepliedAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}

