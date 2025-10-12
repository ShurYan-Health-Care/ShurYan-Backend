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
        [Required(ErrorMessage = "Document URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        public string DocumentUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document type is required")]
        public LaboratoryDocumentType Type { get; set; }
    }
}
