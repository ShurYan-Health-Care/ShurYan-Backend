using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabPrescriptionResponseDto : BaseAuditableDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? GeneralNotes { get; set; }

        public IEnumerable<LabPrescriptionItemDto> Items { get; set; } = new List<LabPrescriptionItemDto>();
    }

}
