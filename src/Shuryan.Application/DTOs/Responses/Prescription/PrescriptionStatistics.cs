using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// General prescription statistics
    /// </summary>
    public class PrescriptionStatistics
    {
        // Time period
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Overall counts
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int DispensedPrescriptions { get; set; }
        public int CancelledPrescriptions { get; set; }
        public int ExpiredPrescriptions { get; set; }
        
        // By entity
        public int UniqueDoctors { get; set; }
        public int UniquePatients { get; set; }
        public int UniquePharmacies { get; set; }
        public int UniqueMedications { get; set; }
        
        // Trends
        public double AveragePrescriptionsPerDay { get; set; }
        public double AverageMedicationsPerPrescription { get; set; }
        public double DigitalShareRate { get; set; } // percentage
        public double DispensingRate { get; set; } // percentage
        
        // Top lists
        public List<TopDoctorStat> TopPrescribingDoctors { get; set; } = new();
        public List<TopMedicationStat> TopPrescribedMedications { get; set; } = new();
        
        // Time series data
        public List<DailyPrescriptionStat> DailyStats { get; set; } = new();
    }

    public class TopDoctorStat
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string? Specialization { get; set; }
        public int PrescriptionCount { get; set; }
    }

    public class DailyPrescriptionStat
    {
        public DateTime Date { get; set; }
        public int PrescriptionsCreated { get; set; }
        public int PrescriptionsDispensed { get; set; }
        public int PrescriptionsCancelled { get; set; }
    }
}
