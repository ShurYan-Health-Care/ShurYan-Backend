using System;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Statistics response for prescriptions analytics
    /// </summary>
    public class PrescriptionStatisticsResponse
    {
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int SharedPrescriptions { get; set; }
        public int OrderedPrescriptions { get; set; }
        public int PrescriptionsToday { get; set; }
        public int PrescriptionsThisWeek { get; set; }
        public int PrescriptionsThisMonth { get; set; }
        public int PrescriptionsThisYear { get; set; }
        public double AverageMedicationsPerPrescription { get; set; }
        public DateTime? LastPrescriptionDate { get; set; }
        public DateTime? FirstPrescriptionDate { get; set; }
    }
}
