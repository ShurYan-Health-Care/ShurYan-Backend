using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLaboratoryDocumentRequest
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public LaboratoryDocumentType Type { get; set; }
    }
}
