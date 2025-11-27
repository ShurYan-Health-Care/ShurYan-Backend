using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UploadLabResultRequest
        {
                [Required(ErrorMessage = "معرف التحليل مطلوب")]
                public Guid LabTestId { get; set; }

                [Required(ErrorMessage = "قيمة النتيجة مطلوبة")]
                [StringLength(500, ErrorMessage = "قيمة النتيجة يجب ألا تتجاوز 500 حرف")]
                public string ResultValue { get; set; } = string.Empty;

                [StringLength(100, ErrorMessage = "المدى المرجعي يجب ألا يتجاوز 100 حرف")]
                public string? ReferenceRange { get; set; }

                [StringLength(50, ErrorMessage = "الوحدة يجب ألا تتجاوز 50 حرف")]
                public string? Unit { get; set; }

                [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
                public string? Notes { get; set; }

                public IFormFile? Attachment { get; set; }
        }

        public class UploadLabResultsRequest
        {
                [Required(ErrorMessage = "النتائج مطلوبة")]
                [MinLength(1, ErrorMessage = "يجب إضافة نتيجة واحدة على الأقل")]
                public List<UploadLabResultRequest> Results { get; set; } = new();
        }
}
