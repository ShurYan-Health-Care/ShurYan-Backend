using System;
using System.Collections.Generic;
using Shuryan.Core.Enums.Appointments;

namespace Shuryan.Application.DTOs.Responses.Doctor
{
    /// <summary>
    /// قائمة توثيق الجلسات للمريض
    /// </summary>
    public class PatientSessionDocumentationListResponse
    {
        public Guid PatientId { get; set; }
        public string PatientFullName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public List<SessionDocumentationResponse> Sessions { get; set; } = new List<SessionDocumentationResponse>();
    }

    /// <summary>
    /// توثيق جلسة واحدة
    /// </summary>
    public class SessionDocumentationResponse
    {
        public Guid AppointmentId { get; set; }
        public Guid ConsultationRecordId { get; set; }
        
        public DateTime SessionDate { get; set; } // تاريخ الجلسة
        public TimeSpan SessionTime { get; set; } // وقت الجلسة
        public ConsultationTypeEnum SessionType { get; set; } // نوع الجلسة (كشف عادي أو إعادة)
        public string SessionTypeName { get; set; } = string.Empty;
        public int SessionDurationMinutes { get; set; } // مدة الجلسة بالدقائق
        
        // محتوى التوثيق
        public string ChiefComplaint { get; set; } = string.Empty; // الشكوى الرئيسية
        public string HistoryOfPresentIllness { get; set; } = string.Empty; // تاريخ المرض الحالي
        public string PhysicalExamination { get; set; } = string.Empty; // الفحص الجسدي
        public string Diagnosis { get; set; } = string.Empty; // التقييم والتشخيص
        public string ManagementPlan { get; set; } = string.Empty; // الخطة العلاجية
        
        public DateTime CreatedAt { get; set; }
    }
}
