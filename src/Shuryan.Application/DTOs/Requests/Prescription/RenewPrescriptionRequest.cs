using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request to renew an existing prescription
    /// </summary>
    public class RenewPrescriptionRequest
    {
        /// <summary>
        /// New general instructions (optional - if not provided, uses original)
        /// </summary>
        [StringLength(1000)]
        public string? GeneralInstructions { get; set; }

        /// <summary>
        /// Duration in days for the renewed prescription
        /// </summary>
        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days")]
        public int DurationInDays { get; set; } = 30;

        /// <summary>
        /// Reason for renewal
        /// </summary>
        [StringLength(500)]
        public string? RenewalReason { get; set; }

        /// <summary>
        /// Whether to copy all medications from original prescription
        /// </summary>
        public bool CopyAllMedications { get; set; } = true;

        /// <summary>
        /// New appointment ID if this renewal is linked to a new appointment
        /// </summary>
        public Guid? NewAppointmentId { get; set; }
    }
}
