using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Pharmacy
{
    public class CreatePharmacyOrderDto
    {
        public Guid PatientId { get; set; }
        public Guid PharmacyId { get; set; }
        public Guid? PrescriptionId { get; set; }
        public OrderDeliveryType DeliveryType { get; set; } 
        public string? DeliveryNotes { get; set; }
    }

}
