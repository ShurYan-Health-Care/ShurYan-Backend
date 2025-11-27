namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LabOrderDetailResponse
        {
                public Guid Id { get; set; }
                public Guid LabPrescriptionId { get; set; }

                // Patient Info
                public Guid PatientId { get; set; }
                public string PatientName { get; set; } = string.Empty;
                public string? PatientPhoneNumber { get; set; }
                public string? PatientEmail { get; set; }

                // Doctor Info
                public string DoctorName { get; set; } = string.Empty;
                public string? DoctorSpecialty { get; set; }

                // Order Info
                public string Status { get; set; } = string.Empty;
                public string StatusArabic { get; set; } = string.Empty;
                public string SampleCollectionType { get; set; } = string.Empty;
                public string SampleCollectionTypeArabic { get; set; } = string.Empty;

                // Costs
                public decimal TestsTotalCost { get; set; }
                public decimal SampleCollectionDeliveryCost { get; set; }
                public decimal TotalCost => TestsTotalCost + SampleCollectionDeliveryCost;

                // Notes
                public string? PrescriptionNotes { get; set; }
                public string? CancellationReason { get; set; }

                // Dates
                public DateTime CreatedAt { get; set; }
                public DateTime? ConfirmedByLabAt { get; set; }
                public DateTime? CancelledAt { get; set; }

                // Tests
                public List<LabOrderTestItemResponse> Tests { get; set; } = new();

                // Results
                public List<LabResultResponse> Results { get; set; } = new();
        }

        public class LabOrderTestItemResponse
        {
                public Guid LabTestId { get; set; }
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;
                public string? SpecialInstructions { get; set; }
                public string? DoctorNotes { get; set; }
        }
}
