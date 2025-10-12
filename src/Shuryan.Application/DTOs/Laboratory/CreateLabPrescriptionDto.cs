using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLabPrescriptionDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string? GeneralNotes { get; set; }
        public IEnumerable<CreateLabPrescriptionItemDto> Items { get; set; } = new List<CreateLabPrescriptionItemDto>();
    }

}
