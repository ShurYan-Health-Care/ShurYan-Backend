using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class AlternativeMedicationRequest
    {
        [Required(ErrorMessage = "اسم الدواء البديل مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الدواء البديل يجب أن يكون أقل من 200 حرف")]
        public string MedicationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }
    }
}
