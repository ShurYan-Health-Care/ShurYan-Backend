using Shuryan.Application.DTOs.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Application.DTOs.Responses.Review
{
    public class DoctorReviewResponse : BaseAuditableDto
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public int OverallSatisfaction { get; set; }
        public int WaitingTime { get; set; }
        public int CommunicationQuality { get; set; }
        public int ClinicCleanliness { get; set; }
        public int ValueForMoney { get; set; }
        public string? Comment { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsEdited { get; set; }
        public string? DoctorReply { get; set; }
        public DateTime? DoctorRepliedAt { get; set; }
        public double AverageRating => (OverallSatisfaction + WaitingTime + CommunicationQuality + ClinicCleanliness + ValueForMoney) / 5.0;
    }
}

