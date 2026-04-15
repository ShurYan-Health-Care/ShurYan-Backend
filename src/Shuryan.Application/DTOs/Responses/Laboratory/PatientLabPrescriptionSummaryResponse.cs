using System;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
    /// <summary>
    /// ملخص التحاليل المطلوبة من مريض معين
    /// </summary>
    public class PatientLabPrescriptionSummaryResponse
    {
        public Guid PrescriptionId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public Guid? LabOrderId { get; set; }
    }
}
