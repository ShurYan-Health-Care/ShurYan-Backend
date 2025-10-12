using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Clinic
{
    public class ClinicPhotoDto : BaseAuditableDto
    {
        public string PhotoUrl { get; set; } = string.Empty;
        public Guid ClinicId { get; set; }
    }

}
