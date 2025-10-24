using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Result of sharing a prescription
    /// </summary>
    public class SharePrescriptionResult
    {
        public Guid PrescriptionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        
        // Share details
        public string ShareCode { get; set; } = null!; // 6-digit code or QR code data
        public string ShareUrl { get; set; } = null!; // Deep link URL
        public DateTime SharedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        // QR Code (base64 image)
        public string? QrCodeImage { get; set; }
        
        // Pharmacy info (if shared with specific pharmacy)
        public Guid? PharmacyId { get; set; }
        public string? PharmacyName { get; set; }
    }
}
