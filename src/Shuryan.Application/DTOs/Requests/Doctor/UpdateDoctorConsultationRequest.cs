using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorConsultationRequest
    {
        [Range(0.01, 10000, ErrorMessage = "Consultation fee must be between 0.01 and 10000")]
        public decimal? ConsultationFee { get; set; }

        [Range(15, 480, ErrorMessage = "Session duration must be between 15-480 minutes")]
        public int? SessionDurationMinutes { get; set; }
    }
}
