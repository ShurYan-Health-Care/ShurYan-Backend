using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class CreatePharmacyOrderRequest
    {
        public Guid PharmacyId { get; set; }
        public Guid? PrescriptionId { get; set; }
        public OrderDeliveryType DeliveryType { get; set; }
        public string DeliveryPersonPhone { get; set; } = string.Empty;
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryNotes { get; set; }
    }
}
