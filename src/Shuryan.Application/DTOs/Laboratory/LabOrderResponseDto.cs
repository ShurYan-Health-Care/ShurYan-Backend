using Shuryan.Application.DTOs.Base;
using Shuryan.Application.DTOs.Patient;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class LabOrderResponseDto : BaseAuditableDto
    {
        public Guid LabPrescriptionId { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid PatientId { get; set; }
        public LabOrderStatus Status { get; set; }
        public SampleCollectionType SampleCollectionType { get; set; }
        public decimal TestsTotalCost { get; set; }
        public decimal SampleCollectionDeliveryCost { get; set; }
        public DateTime? ConfirmedByLabAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public PatientBasicDto? Patient { get; set; }
        public LaboratoryBasicDto? Laboratory { get; set; }
        public IEnumerable<LabResultDto> LabResults { get; set; } = new List<LabResultDto>();
    }
}
