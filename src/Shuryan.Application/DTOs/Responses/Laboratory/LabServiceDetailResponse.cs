namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LabServiceDetailResponse
        {
                public Guid Id { get; set; }
                public Guid LabTestId { get; set; }
                public string LabTestName { get; set; } = string.Empty;
                public string LabTestCode { get; set; } = string.Empty;
                public string LabTestCategory { get; set; } = string.Empty;
                public string? SpecialInstructions { get; set; }
                public decimal Price { get; set; }
                public bool IsAvailable { get; set; }
                public string? LabSpecificNotes { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
        }
}
