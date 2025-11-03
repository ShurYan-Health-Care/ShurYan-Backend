using System;
using System.Collections.Generic;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// قائمة الروشتات للمريض
    /// </summary>
    public class PatientPrescriptionsListResponse
    {
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public int TotalPrescriptions { get; set; }
        public List<PatientPrescriptionResponse> Prescriptions { get; set; } = new List<PatientPrescriptionResponse>();
    }

    /// <summary>
    /// روشتة واحدة مع كل الأدوية
    /// </summary>
    public class PatientPrescriptionResponse
    {
        public Guid PrescriptionId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty; // رقم الروشتة
        public DateTime PrescriptionDate { get; set; } // تاريخ الروشتة
        public Guid? AppointmentId { get; set; }
        
        public List<PatientPrescriptionMedicationResponse> Medications { get; set; } = new List<PatientPrescriptionMedicationResponse>();
    }

    /// <summary>
    /// دواء واحد داخل الروشتة
    /// </summary>
    public class PatientPrescriptionMedicationResponse
    {
        public string MedicationName { get; set; } = string.Empty; // اسم الدواء
        public string Dosage { get; set; } = string.Empty; // الجرعة
        public string Frequency { get; set; } = string.Empty; // عدد المرات
        public int DurationDays { get; set; } // المدة بالأيام
        public string? SpecialInstructions { get; set; } // ملاحظات على الدواء
    }
}
