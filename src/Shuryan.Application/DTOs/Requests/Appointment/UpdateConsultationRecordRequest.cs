using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Appointment
{
    public class UpdateConsultationRecordRequest
    {
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Chief complaint must be between 5-1000 characters")]
        public string? ChiefComplaint { get; set; }

        [StringLength(2000, MinimumLength = 5, ErrorMessage = "History must be between 5-2000 characters")]
        public string? HistoryOfPresentIllness { get; set; }

        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Physical examination must be between 5-2000 characters")]
        public string? PhysicalExamination { get; set; }

        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Diagnosis must be between 3-1000 characters")]
        public string? Diagnosis { get; set; }

        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Management plan must be between 5-2000 characters")]
        public string? ManagementPlan { get; set; }
    }
}
