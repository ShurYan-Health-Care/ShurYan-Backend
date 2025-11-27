namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LabOrderListResponse
        {
                public Guid Id { get; set; }
                public string PatientName { get; set; } = string.Empty;
                public string? PatientPhoneNumber { get; set; }
                public string Status { get; set; } = string.Empty;
                public string StatusArabic { get; set; } = string.Empty;
                public string SampleCollectionType { get; set; } = string.Empty;
                public decimal TestsTotalCost { get; set; }
                public decimal SampleCollectionDeliveryCost { get; set; }
                public decimal TotalCost => TestsTotalCost + SampleCollectionDeliveryCost;
                public int TestsCount { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? ConfirmedByLabAt { get; set; }
        }
}
