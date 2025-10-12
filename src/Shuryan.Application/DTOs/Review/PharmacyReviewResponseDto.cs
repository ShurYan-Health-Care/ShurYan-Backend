using Shuryan.Application.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Review
{
    public class PharmacyReviewResponseDto : BaseAuditableDto
    {
        public Guid PharmacyOrderId { get; set; }
        public Guid PatientId { get; set; }
        public Guid PharmacyId { get; set; }
        public int OverallSatisfaction { get; set; }
        public int MedicationAvailability { get; set; }
        public int ServiceQuality { get; set; }
        public int DeliverySpeed { get; set; }
        public int ValueForMoney { get; set; }
        public bool IsEdited { get; set; }
        public double AverageRating => (OverallSatisfaction + MedicationAvailability + ServiceQuality + DeliverySpeed + ValueForMoney) / 5.0;
    }


}
