using Shuryan.Application.DTOs.Common.Pagination;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Prescription
{
    /// <summary>
    /// Request for searching prescriptions with advanced filters
    /// </summary>
    public class SearchPrescriptionsRequest : PaginationParams
    {
        
        public string? PrescriptionNumber { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? MedicationId { get; set; }

        public Guid? AppointmentId { get; set; }

        public bool? IsDigitallyShared { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(200)]
        public string? SearchTerm { get; set; }
    }
}
