using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLaboratoryHomeSampleCollectionRequest
        {
                [Required(ErrorMessage = "يجب تحديد ما إذا كان المعمل يقدم خدمة سحب العينة من البيت")]
                public bool OffersHomeSampleCollection { get; set; }
                [Range(0, 10000, ErrorMessage = "رسوم الخدمة يجب أن تكون بين 0 و 10000")]
                public decimal? HomeSampleCollectionFee { get; set; }
        }
}
