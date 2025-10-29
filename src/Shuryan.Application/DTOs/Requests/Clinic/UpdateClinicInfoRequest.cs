using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class UpdateClinicInfoRequest
    {
        [StringLength(200, ErrorMessage = "اسم العيادة لا يمكن أن يتجاوز 200 حرف")]
        public string? ClinicName { get; set; }

        public List<PhoneNumberDto>? PhoneNumbers { get; set; }

        public List<ServiceItemDto>? Services { get; set; }
    }

    public class PhoneNumberDto
    {
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [MaxLength(20)]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع الهاتف مطلوب")]
        [Range(0, 2, ErrorMessage = "نوع الهاتف يجب أن يكون 0 (Landline), 1 (WhatsApp), أو 2 (Mobile)")]
        public int Type { get; set; } // 0 = Landline, 1 = WhatsApp, 2 = Mobile
    }

    public class ServiceItemDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
