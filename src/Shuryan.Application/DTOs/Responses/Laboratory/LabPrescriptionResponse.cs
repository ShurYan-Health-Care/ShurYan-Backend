using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabPrescriptionResponse : BaseAuditableDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? GeneralNotes { get; set; }
        public IEnumerable<LabPrescriptionItemResponse> Items { get; set; } = new List<LabPrescriptionItemResponse>();
    }
}
