using Shuryan.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Advanced query parameters for prescription search and filtering
    /// </summary>
    public class PrescriptionQueryParams
    {
        // Pagination
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        // Filtering
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? PharmacyId { get; set; }
        public Guid? MedicationId { get; set; }
        public Guid? AppointmentId { get; set; }

        // Status filtering
        public PrescriptionStatus? Status { get; set; }

        // Date range
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Search
        [StringLength(200)]
        public string? SearchTerm { get; set; } // Search in prescription number, patient name, doctor name

        // Sorting
        public string? SortBy { get; set; } // CreatedAt, UpdatedAt, PrescriptionNumber
        public bool SortDescending { get; set; } = true;

        /// <summary>
        /// Validates the query parameters
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            {
                errorMessage = "Start date cannot be greater than end date";
                return false;
            }

            if (Page < 1)
            {
                errorMessage = "Page must be greater than 0";
                return false;
            }

            if (PageSize < 1 || PageSize > 100)
            {
                errorMessage = "Page size must be between 1 and 100";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
