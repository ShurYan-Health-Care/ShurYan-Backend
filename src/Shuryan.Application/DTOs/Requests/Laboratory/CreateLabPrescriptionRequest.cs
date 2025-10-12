using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLabPrescriptionRequest
    {
        [Required(ErrorMessage = "Appointment ID is required")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public Guid PatientId { get; set; }

        [StringLength(1000, ErrorMessage = "General notes cannot exceed 1000 characters")]
        public string? GeneralNotes { get; set; }

        [Required(ErrorMessage = "At least one lab test item is required")]
        [MinLength(1, ErrorMessage = "At least one lab test item is required")]
        public IEnumerable<CreateLabPrescriptionItemRequest> Items { get; set; } = new List<CreateLabPrescriptionItemRequest>();
    }
}
