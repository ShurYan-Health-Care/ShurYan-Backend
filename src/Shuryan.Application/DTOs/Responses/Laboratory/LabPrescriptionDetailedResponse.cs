using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    /// <summary>
    /// تفاصيل روشتة التحاليل الكاملة
    /// </summary>
    public class LabPrescriptionDetailedResponse : BaseAuditableDto
    {
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? GeneralNotes { get; set; }
        public IEnumerable<LabPrescriptionItemDetailedResponse> Items { get; set; } = new List<LabPrescriptionItemDetailedResponse>();
    }

    public class LabPrescriptionItemDetailedResponse
    {
        public Guid Id { get; set; }
        public Guid LabTestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
