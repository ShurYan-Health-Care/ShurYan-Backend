using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// طلب تحاليل للمريض
        /// </summary>
        public class PatientLabOrderResponse
        {
                public Guid Id { get; set; }
                public Guid LabPrescriptionId { get; set; }

                // Laboratory Info
                public Guid LaboratoryId { get; set; }
                public string LaboratoryName { get; set; } = string.Empty;
                public string? LaboratoryProfileImage { get; set; }
                public string? LaboratoryPhone { get; set; }
                public double? LaboratoryRating { get; set; }

                // Status
                public string Status { get; set; } = string.Empty;
                public string StatusArabic { get; set; } = string.Empty;

                // Collection Type
                public string SampleCollectionType { get; set; } = string.Empty;
                public string SampleCollectionTypeArabic { get; set; } = string.Empty;

                // Costs
                public decimal TestsTotalCost { get; set; }
                public decimal SampleCollectionDeliveryCost { get; set; }
                public decimal TotalCost => TestsTotalCost + SampleCollectionDeliveryCost;

                // Dates
                public DateTime CreatedAt { get; set; }
                public DateTime? ConfirmedByLabAt { get; set; }
                public DateTime? CancelledAt { get; set; }
                public string? CancellationReason { get; set; }

                // Tests
                public List<PatientLabOrderTestResponse> Tests { get; set; } = new();

                // Results
                public bool HasResults { get; set; }
                public int ResultsCount { get; set; }
        }

        public class PatientLabOrderTestResponse
        {
                public Guid LabTestId { get; set; }
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;
                public decimal Price { get; set; }
                public bool HasResult { get; set; }
        }
}
