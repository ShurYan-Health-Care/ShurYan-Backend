using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Doctor
{
    public class UpdateDoctorDocumentRequest
    {
        public string? DocumentUrl { get; set; }
        public DoctorDocumentType? Type { get; set; }
    }
}
