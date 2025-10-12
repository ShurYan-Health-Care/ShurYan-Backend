using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Requests.Review
{
    public class UpdateDoctorReviewRequest
    {
        public int OverallSatisfaction { get; set; }
        public int WaitingTime { get; set; }
        public int CommunicationQuality { get; set; }
        public int ClinicCleanliness { get; set; }
        public int ValueForMoney { get; set; }
        public string? Comment { get; set; }
    }
}

