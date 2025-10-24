using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request to dispense a prescription at a pharmacy
    /// </summary>
    public class DispensePrescriptionRequest
    {
        [Required]
        public Guid PharmacyId { get; set; }

        [Required]
        public Guid PharmacistId { get; set; }

        /// <summary>
        /// List of medications dispensed with quantities
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one medication must be dispensed")]
        public List<DispensedMedicationItem> DispensedMedications { get; set; } = new();

        /// <summary>
        /// Total cost of dispensed medications
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Payment method
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

       
        [StringLength(1000)]
        public string? PharmacistNotes { get; set; }

        /// <summary>
        /// Patient signature confirmation
        /// </summary>
        public bool PatientSignatureConfirmed { get; set; }
    }

    public class DispensedMedicationItem
    {
        [Required]
        public Guid MedicationId { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantityDispensed { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(200)]
        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
