using System.ComponentModel.DataAnnotations;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLaboratoryAddressRequest
        {
                public Governorate? Governorate { get; set; }

                [StringLength(100, ErrorMessage = "اسم المدينة يجب ألا يتجاوز 100 حرف")]
                public string? City { get; set; }

                [StringLength(200, ErrorMessage = "اسم الشارع يجب ألا يتجاوز 200 حرف")]
                public string? Street { get; set; }

                [StringLength(20, ErrorMessage = "رقم المبنى يجب ألا يتجاوز 20 حرف")]
                public string? BuildingNumber { get; set; }

                [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
                public double? Latitude { get; set; }

                [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
                public double? Longitude { get; set; }
        }
}
