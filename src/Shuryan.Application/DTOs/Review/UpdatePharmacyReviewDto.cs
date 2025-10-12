using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Review
{
    public class UpdatePharmacyReviewDto
    {
        public int OverallSatisfaction { get; set; }
        public int MedicationAvailability { get; set; }
        public int ServiceQuality { get; set; }
        public int DeliverySpeed { get; set; }
        public int ValueForMoney { get; set; }
    }
}
