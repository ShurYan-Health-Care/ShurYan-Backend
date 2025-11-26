using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Patient
{
        /// <summary>
        /// تقييم معمل
        /// </summary>
        public class CreateLaboratoryReviewRequest
        {
                [Required(ErrorMessage = "تقييم الرضا العام مطلوب")]
                [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
                public int OverallSatisfaction { get; set; }

                [Required(ErrorMessage = "تقييم دقة النتائج مطلوب")]
                [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
                public int ResultAccuracy { get; set; }

                [Required(ErrorMessage = "تقييم سرعة التسليم مطلوب")]
                [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
                public int DeliverySpeed { get; set; }

                [Required(ErrorMessage = "تقييم جودة الخدمة مطلوب")]
                [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
                public int ServiceQuality { get; set; }

                [Required(ErrorMessage = "تقييم القيمة مقابل السعر مطلوب")]
                [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5")]
                public int ValueForMoney { get; set; }
        }
}
