using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Prescription
{
    /// <summary>
    /// Statistics for a pharmacy
    /// </summary>
    public class PharmacyStatistics
    {
        public Guid PharmacyId { get; set; }
        public string PharmacyName { get; set; } = null!;
        
        // Time period
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Prescription stats
        public int TotalPrescriptionsReceived { get; set; }
        public int TotalPrescriptionsDispensed { get; set; }
        public int PendingPrescriptions { get; set; }
        public int CancelledPrescriptions { get; set; }
        
        // Financial stats
        public decimal TotalRevenue { get; set; }
        public decimal AverageTransactionValue { get; set; }
        
        // Top medications
        public List<TopMedicationStat> TopMedications { get; set; } = new();
        
        // Performance metrics
        public double AverageDispensingTime { get; set; } // in minutes
        public double CustomerSatisfactionRate { get; set; }
        
        // Daily breakdown
        public List<DailyPharmacyStat> DailyStats { get; set; } = new();
    }

    public class TopMedicationStat
    {
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; } = null!;
        public int TimesDispensed { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyPharmacyStat
    {
        public DateTime Date { get; set; }
        public int PrescriptionsDispensed { get; set; }
        public decimal Revenue { get; set; }
    }
}
