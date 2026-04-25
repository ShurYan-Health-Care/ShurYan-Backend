using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
        public class UpdateLaboratoryBasicInfoRequest
        {
                [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم المعمل يجب أن يكون بين 2-100 حرف")]
                public string? Name { get; set; }

                [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
                public string? Description { get; set; }

                [RegularExpression("^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح يتكون من 11 رقم ويبدأ بـ 010 أو 011 أو 012 أو 015")]
                public string? PhoneNumber { get; set; }
                [Phone(ErrorMessage = "صيغة رقم الواتساب غير صحيحة")]
                [StringLength(20, MinimumLength = 10, ErrorMessage = "رقم الواتساب يجب أن يكون بين 10-20 رقم")]
                public string? WhatsAppNumber { get; set; }

                [Url(ErrorMessage = "صيغة الموقع الإلكتروني غير صحيحة")]
                [StringLength(200, ErrorMessage = "الموقع الإلكتروني يجب ألا يتجاوز 200 حرف")]
                public string? Website { get; set; }
        }
}
