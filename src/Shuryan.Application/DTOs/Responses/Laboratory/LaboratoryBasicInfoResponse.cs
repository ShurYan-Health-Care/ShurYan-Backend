namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        public class LaboratoryBasicInfoResponse
        {
                public string Name { get; set; } = string.Empty;
                public string? Description { get; set; }
                public string Email { get; set; } = string.Empty;
                public string? PhoneNumber { get; set; }
                public string? WhatsAppNumber { get; set; }
                public string? Website { get; set; }

                public string? ProfileImageUrl { get; set; }
        }
}
