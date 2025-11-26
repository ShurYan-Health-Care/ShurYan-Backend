using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Patient
{
        /// <summary>
        /// طلب خدمة سحب العينة من البيت
        /// </summary>
        public class RequestHomeSampleCollectionRequest
        {
                [Required(ErrorMessage = "عنوان السحب مطلوب")]
                [StringLength(500, ErrorMessage = "العنوان يجب ألا يتجاوز 500 حرف")]
                public string Address { get; set; } = string.Empty;

                public double? Latitude { get; set; }
                public double? Longitude { get; set; }

                [Required(ErrorMessage = "تاريخ ووقت السحب مطلوب")]
                public DateTime PreferredDateTime { get; set; }

                [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
                public string? Notes { get; set; }

                [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
                public string? ContactPhone { get; set; }
        }
}
