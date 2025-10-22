using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Audit trail entry for prescription status changes
    /// </summary>
    public class PrescriptionStatusHistory
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        
        // Status change
        public string PreviousStatus { get; set; } = null!;
        public string NewStatus { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
        
        // Who made the change
        public Guid? ChangedBy { get; set; }
        public string? ChangedByName { get; set; }
        public string? ChangedByRole { get; set; } // Doctor, Pharmacist, System, Patient
        
        // Reason/Notes
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        
        // Additional context
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
