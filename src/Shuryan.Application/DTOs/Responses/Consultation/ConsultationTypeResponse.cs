using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Consultation
{
    public class ConsultationTypeResponse
    {
        public Guid Id { get; set; }
        public ConsultationTypeEnum ConsultationTypeEnum { get; set; }
    }
}
