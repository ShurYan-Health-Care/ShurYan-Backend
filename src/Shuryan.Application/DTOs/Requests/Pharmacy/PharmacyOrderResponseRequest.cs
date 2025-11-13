using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class PharmacyOrderResponseRequest
    {
        [Required(ErrorMessage = "قائمة الأدوية مطلوبة")]
        [MinLength(1, ErrorMessage = "يجب أن تحتوي على دواء واحد على الأقل")]
        public List<MedicationAvailabilityRequest> Medications { get; set; } = new();

        [Required(ErrorMessage = "المبلغ الإجمالي مطلوب")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ الإجمالي يجب أن يكون صفر أو أكبر")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "حالة توفر التوصيل مطلوبة")]
        public bool DeliveryAvailable { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "رسوم التوصيل يجب أن تكون صفر أو أكبر")]
        public decimal DeliveryFee { get; set; }

        [MaxLength(1000, ErrorMessage = "ملاحظات الصيدلية يجب أن تكون أقل من 1000 حرف")]
        public string? PharmacyNotes { get; set; }
    }
}
