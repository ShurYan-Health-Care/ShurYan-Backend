using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Review
{
    public class CreateLaboratoryReviewRequest
    {
        [Required(ErrorMessage = "Lab order ID is required")]
        public Guid LabOrderId { get; set; }

        [Required(ErrorMessage = "Overall satisfaction rating is required")]
        [Range(1, 5, ErrorMessage = "Overall satisfaction must be between 1-5")]
        public int OverallSatisfaction { get; set; }

        [Required(ErrorMessage = "Result accuracy rating is required")]
        [Range(1, 5, ErrorMessage = "Result accuracy must be between 1-5")]
        public int ResultAccuracy { get; set; }

        [Required(ErrorMessage = "Delivery speed rating is required")]
        [Range(1, 5, ErrorMessage = "Delivery speed must be between 1-5")]
        public int DeliverySpeed { get; set; }

        [Required(ErrorMessage = "Service quality rating is required")]
        [Range(1, 5, ErrorMessage = "Service quality must be between 1-5")]
        public int ServiceQuality { get; set; }

        [Required(ErrorMessage = "Value for money rating is required")]
        [Range(1, 5, ErrorMessage = "Value for money must be between 1-5")]
        public int ValueForMoney { get; set; }
    }
}
