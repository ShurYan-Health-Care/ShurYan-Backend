using System;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.DTOs.Responses.Emergency
{
    public class EmergencyEventResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public EmergencyEventStatus Status { get; set; }

        // Patient vitals — included for quick access in emergency dashboard
        public BloodType? BloodType { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public bool IsPregnant { get; set; }
        public string? PhysicalDisabilities { get; set; }

        // Emergency contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
    }
}
