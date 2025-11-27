using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums.Laboratory;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLabOrderStatusRequest
        {
                [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
                public LabOrderStatus NewStatus { get; set; }

                [StringLength(500, ErrorMessage = "السبب يجب ألا يتجاوز 500 حرف")]
                public string? Reason { get; set; }
        }
}
