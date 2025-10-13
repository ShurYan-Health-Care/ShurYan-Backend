using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Pharmacy
{
    public class UpdatePharmacyRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public bool? OffersDelivery { get; set; }
    }
}

