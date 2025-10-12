using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class CreateDoctorConsultationDto
    {
        public ConsultationTypeEnum ConsultationType { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public int SessionDurationMinutes { get; set; }
    }

}
