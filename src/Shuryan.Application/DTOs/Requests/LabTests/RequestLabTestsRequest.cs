using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.LabTests
{
    /// <summary>
    /// Request لطلب تحاليل طبية
    /// </summary>
    public class RequestLabTestsRequest
    {
        [Required(ErrorMessage = "يجب إضافة تحليل واحد على الأقل")]
        [MinLength(1, ErrorMessage = "يجب إضافة تحليل واحد على الأقل")]
        public List<string> Tests { get; set; } = new List<string>();

        [MaxLength(1000, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 1000 حرف")]
        public string? Notes { get; set; }
    }
}
