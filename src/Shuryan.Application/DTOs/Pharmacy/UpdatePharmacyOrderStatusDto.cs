using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class UpdatePharmacyOrderStatusDto
    {
        public PharmacyOrderStatus Status { get; set; }
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryPersonPhone { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
    }

}
