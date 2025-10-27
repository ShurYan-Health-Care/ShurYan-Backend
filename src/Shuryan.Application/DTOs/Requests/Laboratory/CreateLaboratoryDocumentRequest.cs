using Microsoft.AspNetCore.Http;
using Shuryan.Core.Enums.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class CreateLaboratoryDocumentRequest
    {
        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; } = null!;

        [Required(ErrorMessage = "Document type is required")]
        public LaboratoryDocumentType Type { get; set; }
    }
}
