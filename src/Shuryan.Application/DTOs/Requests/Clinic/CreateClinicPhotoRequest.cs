using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Clinic
{
    public class CreateClinicPhotoRequest
    {
        public string PhotoUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
