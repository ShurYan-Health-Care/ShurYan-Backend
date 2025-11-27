namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// نتيجة تحليل للمريض
        /// </summary>
        public class PatientLabResultResponse
        {
                public Guid Id { get; set; }
                public Guid LabOrderId { get; set; }
                public Guid LabTestId { get; set; }

                // Test Info
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;

                // Result
                public string ResultValue { get; set; } = string.Empty;
                public string? ReferenceRange { get; set; }
                public string? Unit { get; set; }
                public string? Notes { get; set; }
                public string? AttachmentUrl { get; set; }

                public DateTime CreatedAt { get; set; }
        }
}
