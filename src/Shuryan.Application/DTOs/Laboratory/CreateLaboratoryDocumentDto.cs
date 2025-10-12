using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Laboratory
{
    public class CreateLaboratoryDocumentDto
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
    }
}
