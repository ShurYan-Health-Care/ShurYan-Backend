using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class AddLabServiceRequest
        {
                [Required(ErrorMessage = "معرف التحليل مطلوب")]
                public Guid LabTestId { get; set; }

                [Required(ErrorMessage = "السعر مطلوب")]
                [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون بين 0.01 و 100000")]
                public decimal Price { get; set; }

                public bool IsAvailable { get; set; } = true;

                [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
                public string? LabSpecificNotes { get; set; }
        }
}
