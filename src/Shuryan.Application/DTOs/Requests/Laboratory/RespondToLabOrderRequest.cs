using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class RespondToLabOrderRequest
        {
                [Required(ErrorMessage = "يجب تحديد قبول أو رفض الطلب")]
                public bool Accept { get; set; }

                [StringLength(500, ErrorMessage = "سبب الرفض يجب ألا يتجاوز 500 حرف")]
                public string? RejectionReason { get; set; }

                [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
                public string? Notes { get; set; }
        }
}
