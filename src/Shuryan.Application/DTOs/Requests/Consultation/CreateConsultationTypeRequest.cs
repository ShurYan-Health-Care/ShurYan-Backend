using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Consultation
{
    public class CreateConsultationTypeRequest
    {
        public ConsultationTypeEnum ConsultationTypeEnum { get; set; }
    }
}
