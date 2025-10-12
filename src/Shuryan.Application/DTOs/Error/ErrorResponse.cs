using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Error
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<ValidationError>? Errors { get; set; }
        public string? TraceId { get; set; }
    }


}
