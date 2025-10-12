using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    public class DoctorConsultationResponse : BaseAuditableDto
    {
        public Guid DoctorId { get; set; }
        public ConsultationTypeEnum ConsultationType { get; set; }
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }
    }
}

