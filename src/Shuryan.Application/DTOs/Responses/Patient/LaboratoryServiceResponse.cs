namespace Shuryan.Application.DTOs.Responses.Patient
{
        /// <summary>
        /// خدمة/تحليل متوفر في معمل
        /// </summary>
        public class LaboratoryServiceResponse
        {
                public Guid Id { get; set; }
                public Guid LabTestId { get; set; }
                public string TestName { get; set; } = string.Empty;
                public string TestCode { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;
                public string CategoryArabic { get; set; } = string.Empty;
                public decimal Price { get; set; }
                public bool IsAvailable { get; set; }
                public string? SpecialInstructions { get; set; }
                public string? LaboratoryNotes { get; set; }
        }
}
