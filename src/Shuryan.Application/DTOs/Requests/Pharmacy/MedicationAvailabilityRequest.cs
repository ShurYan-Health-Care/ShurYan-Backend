using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class MedicationAvailabilityRequest
    {
        [Required(ErrorMessage = "اسم الدواء مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الدواء يجب أن يكون أقل من 200 حرف")]
        public string MedicationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "حالة التوفر مطلوبة")]
        public bool IsAvailable { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "الكمية المتاحة يجب أن تكون صفر أو أكبر")]
        public int AvailableQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون صفر أو أكبر")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// البديل الوحيد المتاح (اختياري)
        /// </summary>
        public AlternativeMedicationRequest? AlternativeOne { get; set; }
    }
}
