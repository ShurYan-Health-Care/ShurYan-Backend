using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Clinic
{
    public class ClinicInfoResponse
    {
        public string ClinicName { get; set; } = string.Empty;
        public List<PhoneNumberResponse> PhoneNumbers { get; set; } = new List<PhoneNumberResponse>();
        public List<ServiceItemResponse> Services { get; set; } = new List<ServiceItemResponse>();
    }

    public class PhoneNumberResponse
    {
        public string Number { get; set; } = string.Empty;
        public int Type { get; set; } // 0 = Landline, 1 = WhatsApp, 2 = Mobile
    }

    public class ServiceItemResponse
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
