using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class UpdateServicePricingRequest
    {
        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "المدة مطلوبة")]
        [Range(5, 120, ErrorMessage = "المدة يجب أن تكون بين 5 و 120 دقيقة")]
        public int Duration { get; set; }
    }
}
