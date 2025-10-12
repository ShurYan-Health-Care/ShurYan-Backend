using Shuryan.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Patient
{
    public class UpdateMedicalHistoryItemDto
    {
        public MedicalHistoryType? Type { get; set; }
        public string? Text { get; set; }
    }
}
