using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Review
{
    public class LaboratoryReviewResponse : BaseAuditableDto
    {
        public Guid LabOrderId { get; set; }
        public Guid PatientId { get; set; }
        public Guid LaboratoryId { get; set; }
        public int OverallSatisfaction { get; set; }
        public int ResultAccuracy { get; set; }
        public int DeliverySpeed { get; set; }
        public int ServiceQuality { get; set; }
        public int ValueForMoney { get; set; }
        public bool IsEdited { get; set; }
        public double AverageRating => (OverallSatisfaction + ResultAccuracy + DeliverySpeed + ServiceQuality + ValueForMoney) / 5.0;
    }
}
