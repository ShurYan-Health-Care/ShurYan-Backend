using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request to share a prescription digitally with a pharmacy
    /// </summary>
    public class SharePrescriptionRequest
    {
        /// <summary>
        /// Pharmacy ID to share with (optional - if not provided, generates a shareable code)
        /// </summary>
        public Guid? PharmacyId { get; set; }

        /// <summary>
        /// Expiration time for the share link/code (in hours)
        /// </summary>
        [Range(1, 168, ErrorMessage = "Expiration must be between 1 and 168 hours (7 days)")]
        public int ExpirationHours { get; set; } = 24;

        /// <summary>
        /// Whether to allow multiple pharmacies to view this prescription
        /// </summary>
        public bool AllowMultipleViews { get; set; } = false;

        /// <summary>
        /// Optional message to pharmacy
        /// </summary>
        [StringLength(500)]
        public string? MessageToPharmacy { get; set; }
    }
}
