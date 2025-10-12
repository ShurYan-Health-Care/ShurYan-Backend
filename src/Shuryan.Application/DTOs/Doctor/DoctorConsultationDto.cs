using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class DoctorConsultationDto : BaseAuditableDto
    {
        public Guid DoctorId { get; set; }
        public ConsultationTypeEnum ConsultationType { get; set; }
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }
    }

}
