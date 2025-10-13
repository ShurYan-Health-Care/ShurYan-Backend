using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Review
{
    public class UpdateDoctorReviewRequest
    {
        [Required(ErrorMessage = "Overall satisfaction rating is required")]
        [Range(1, 5, ErrorMessage = "Overall satisfaction must be between 1-5")]
        public int OverallSatisfaction { get; set; }

        [Required(ErrorMessage = "Waiting time rating is required")]
        [Range(1, 5, ErrorMessage = "Waiting time must be between 1-5")]
        public int WaitingTime { get; set; }

        [Required(ErrorMessage = "Communication quality rating is required")]
        [Range(1, 5, ErrorMessage = "Communication quality must be between 1-5")]
        public int CommunicationQuality { get; set; }

        [Required(ErrorMessage = "Clinic cleanliness rating is required")]
        [Range(1, 5, ErrorMessage = "Clinic cleanliness must be between 1-5")]
        public int ClinicCleanliness { get; set; }

        [Required(ErrorMessage = "Value for money rating is required")]
        [Range(1, 5, ErrorMessage = "Value for money must be between 1-5")]
        public int ValueForMoney { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string? Comment { get; set; }
    }
}

