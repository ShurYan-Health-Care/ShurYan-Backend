using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LabTestListResponse
        {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string Code { get; set; } = string.Empty;
                public string Category { get; set; } = string.Empty;
                public string? SpecialInstructions { get; set; }
        }
}
