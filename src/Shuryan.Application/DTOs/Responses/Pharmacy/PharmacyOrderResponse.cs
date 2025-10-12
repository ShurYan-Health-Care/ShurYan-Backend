using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Pharmacy
{
    public class PharmacyOrderResponse : BaseAuditableDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public PharmacyOrderStatus Status { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public OrderDeliveryType DeliveryType { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public string DeliveryPersonPhone { get; set; } = string.Empty;
        public string? DeliveryPersonName { get; set; }
        public string? DeliveryNotes { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public Guid PatientId { get; set; }
        public Guid PharmacyId { get; set; }
        public Guid? PrescriptionId { get; set; }
    }
}
