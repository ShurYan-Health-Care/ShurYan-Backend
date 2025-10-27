using Microsoft.AspNetCore.Http;
using Shuryan.Core.Enums.Pharmacy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class CreatePharmacyDocumentRequest
    {
        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; } = null!;

        [Required(ErrorMessage = "Document type is required")]
        public PharmacyDocumentType Type { get; set; }
    }
}
