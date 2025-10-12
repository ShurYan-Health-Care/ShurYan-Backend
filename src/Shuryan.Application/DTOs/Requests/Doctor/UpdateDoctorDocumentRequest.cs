using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorDocumentRequest
    {
        [Url(ErrorMessage = "Invalid URL format")]
        public string? DocumentUrl { get; set; }

        public DoctorDocumentType? Type { get; set; }
    }
}
