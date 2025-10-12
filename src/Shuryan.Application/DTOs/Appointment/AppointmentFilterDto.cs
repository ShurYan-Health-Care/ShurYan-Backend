using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Appointment
{
    public class AppointmentFilterDto : PaginationParams
    {
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
