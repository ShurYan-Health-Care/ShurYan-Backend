using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Laboratory
{
    public class UpdateLaboratoryRequest
    {
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Laboratory name must be between 3-200 characters")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Phone(ErrorMessage = "Invalid WhatsApp number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "WhatsApp number must be between 10-20 characters")]
        public string? WhatsAppNumber { get; set; }

        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        public bool? OffersHomeSampleCollection { get; set; }

        [Range(0, 10000, ErrorMessage = "Home sample collection fee must be between 0 and 10000")]
        public decimal? HomeSampleCollectionFee { get; set; }

        public IEnumerable<CreateLabWorkingHoursRequest>? WorkingHours { get; set; }
    }
}

