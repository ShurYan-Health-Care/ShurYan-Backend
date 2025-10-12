using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Review
{
    public class CreateLaboratoryReviewRequest
    {
        public Guid LabOrderId { get; set; }
        public int OverallSatisfaction { get; set; }
        public int ResultAccuracy { get; set; }
        public int DeliverySpeed { get; set; }
        public int ServiceQuality { get; set; }
        public int ValueForMoney { get; set; }
    }
}
