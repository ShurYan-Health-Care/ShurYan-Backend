using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Patient
{
        /// <summary>
        /// البحث عن معامل قريبة
        /// </summary>
        public class NearbyLaboratoriesRequest
        {
                [Required(ErrorMessage = "خط العرض مطلوب")]
                [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و 90")]
                public double Latitude { get; set; }

                [Required(ErrorMessage = "خط الطول مطلوب")]
                [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و 180")]
                public double Longitude { get; set; }

                [Range(1, 100, ErrorMessage = "المسافة يجب أن تكون بين 1 و 100 كم")]
                public double RadiusInKm { get; set; } = 10;

                public bool? OffersHomeSampleCollection { get; set; }

                public string? SearchQuery { get; set; }

                [Range(1, 100)]
                public int PageNumber { get; set; } = 1;

                [Range(1, 50)]
                public int PageSize { get; set; } = 20;
        }
}
