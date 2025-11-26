using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLabServiceRequest
        {
                [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون بين 0.01 و 100000")]
                public decimal? Price { get; set; }

                [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
                public string? LabSpecificNotes { get; set; }
        }
}
