using System;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.DTOs.Responses.Emergency
{
    public class EmergencyPatientResponse
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public BloodType? BloodType { get; set; }
        public decimal? WeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public bool IsPregnant { get; set; }
        public string? PhysicalDisabilities { get; set; }
        public DateTime? EmergencyModeActivatedAt { get; set; }
    }
}
