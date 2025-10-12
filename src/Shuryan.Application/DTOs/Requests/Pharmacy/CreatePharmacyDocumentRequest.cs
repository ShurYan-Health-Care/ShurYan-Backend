using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class CreatePharmacyDocumentRequest
    {
        public string DocumentUrl { get; set; } = string.Empty;
        public PharmacyDocumentType Type { get; set; }
    }
}
