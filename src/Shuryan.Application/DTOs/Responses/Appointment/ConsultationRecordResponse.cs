using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Appointment
{
    public class ConsultationRecordResponse : BaseAuditableDto
    {
        public Guid AppointmentId { get; set; }
        public string ChiefComplaint { get; set; } = string.Empty;
        public string HistoryOfPresentIllness { get; set; } = string.Empty;
        public string PhysicalExamination { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string ManagementPlan { get; set; } = string.Empty;
    }
}
