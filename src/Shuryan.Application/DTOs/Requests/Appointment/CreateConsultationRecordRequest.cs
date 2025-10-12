using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class CreateConsultationRecordRequest
    {
        [Required(ErrorMessage = "Appointment ID is required")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "Chief complaint is required")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Chief complaint must be between 5-1000 characters")]
        public string ChiefComplaint { get; set; } = string.Empty;

        [Required(ErrorMessage = "History of present illness is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "History must be between 5-2000 characters")]
        public string HistoryOfPresentIllness { get; set; } = string.Empty;

        [Required(ErrorMessage = "Physical examination is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Physical examination must be between 5-2000 characters")]
        public string PhysicalExamination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Diagnosis must be between 3-1000 characters")]
        public string Diagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Management plan is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Management plan must be between 5-2000 characters")]
        public string ManagementPlan { get; set; } = string.Empty;
    }
}
