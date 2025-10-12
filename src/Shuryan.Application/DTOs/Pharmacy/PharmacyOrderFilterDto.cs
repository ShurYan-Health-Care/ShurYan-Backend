using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class PharmacyOrderFilterDto : PaginationParams
    {
        public Guid? PatientId { get; set; }
        public Guid? PharmacyId { get; set; }
        public PharmacyOrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }


}
