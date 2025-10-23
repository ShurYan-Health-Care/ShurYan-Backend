using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    public class LabOrderResponse : BaseAuditableDto
    {
        public Guid LabPrescriptionId { get; set; }
        public Guid LaboratoryId { get; set; }
        public string LaboratoryName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public LabOrderStatus Status { get; set; }
        public SampleCollectionType SampleCollectionType { get; set; }
        public decimal TestsTotalCost { get; set; }
        public decimal SampleCollectionDeliveryCost { get; set; }
        public DateTime? ConfirmedByLabAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public IEnumerable<LabOrderTestResponse>? Tests { get; set; }
    }
}
