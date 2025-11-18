using System;

namespace Shuryan.Application.DTOs.Responses.Review
{
    public class DoctorReviewListItemResponse
    {
        public Guid ReviewId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientProfileImage { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

