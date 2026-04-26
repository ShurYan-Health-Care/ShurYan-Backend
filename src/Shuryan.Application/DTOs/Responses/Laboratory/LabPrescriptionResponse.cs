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
        public string DoctorName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public string QrCodeUrl => $"https://shuryan.com/rx/{Id}";
        public IEnumerable<LabPrescriptionItemResponse> Items { get; set; } = new List<LabPrescriptionItemResponse>();
    }
}
