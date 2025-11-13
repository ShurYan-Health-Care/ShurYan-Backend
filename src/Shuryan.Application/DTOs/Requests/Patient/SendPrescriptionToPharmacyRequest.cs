using System;
using System.ComponentModel.DataAnnotations;

namespace Shuryan.Application.DTOs.Requests.Patient
{
    public class SendPrescriptionToPharmacyRequest
    {
        [Required(ErrorMessage = "معرف الصيدلية مطلوب")]
        public Guid PharmacyId { get; set; }
    }
}
