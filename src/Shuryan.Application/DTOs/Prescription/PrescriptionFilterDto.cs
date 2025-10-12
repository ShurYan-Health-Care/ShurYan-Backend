using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Prescription
{
    public class PrescriptionFilterDto : PaginationParams
    {
        public Guid? PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
