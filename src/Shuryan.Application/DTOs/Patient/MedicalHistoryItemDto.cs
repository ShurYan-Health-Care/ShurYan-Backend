using Shuryan.Application.DTOs.Base;
using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Patient
{
    public class MedicalHistoryItemDto : BaseAuditableDto
    {
        public MedicalHistoryType Type { get; set; } 
        public string Text { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
    }
}
