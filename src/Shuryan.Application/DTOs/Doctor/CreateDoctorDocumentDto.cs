using Shuryan.Core.Enums.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Doctor
{
    public class CreateDoctorDocumentDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public DoctorDocumentType Type { get; set; }
    }

}
