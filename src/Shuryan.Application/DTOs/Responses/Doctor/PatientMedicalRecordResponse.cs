using System;
using System.Collections.Generic;
using Shuryan.Core.Enums;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// السجل الطبي للمريض - بسيط وواضح
    /// بيرجع كل الـ MedicalHistoryItems بتاعت المريض
    /// </summary>
    public class PatientMedicalRecordResponse
    {
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public List<MedicalHistoryItemResponse> MedicalHistory { get; set; } = new List<MedicalHistoryItemResponse>();
    }

    /// <summary>
    /// عنصر واحد من السجل الطبي
    /// النوع (Type) + النص (Text)
    /// </summary>
    public class MedicalHistoryItemResponse
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// نوع المعلومة الطبية
        /// DrugAllergy = الحساسية من الأدوية
        /// ChronicDisease = الأمراض المزمنة
        /// CurrentMedication = الأدوية الحالية
        /// PreviousSurgery = العمليات الجراحية السابقة
        /// </summary>
        public MedicalHistoryType Type { get; set; }
        
        /// <summary>
        /// اسم النوع بالعربي
        /// </summary>
        public string TypeName { get; set; } = string.Empty;
        
        /// <summary>
        /// التفاصيل - النص اللي الدكتور كتبه
        /// </summary>
        public string Text { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
