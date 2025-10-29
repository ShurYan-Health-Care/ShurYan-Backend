using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class UpdateClinicAddressRequest
    {
        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المحافظة لا يمكن أن يتجاوز 100 حرف")]
        public string Governorate { get; set; } = string.Empty;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        [StringLength(100, ErrorMessage = "اسم المدينة لا يمكن أن يتجاوز 100 حرف")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "الشارع مطلوب")]
        [StringLength(200, ErrorMessage = "اسم الشارع لا يمكن أن يتجاوز 200 حرف")]
        public string Street { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "رقم المبنى لا يمكن أن يتجاوز 50 حرف")]
        public string? BuildingNumber { get; set; }

        [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
        public double? Longitude { get; set; }
    }
}
