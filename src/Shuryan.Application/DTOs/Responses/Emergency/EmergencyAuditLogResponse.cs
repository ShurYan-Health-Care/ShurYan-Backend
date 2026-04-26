using System;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.DTOs.Responses.Emergency
{
    public class EmergencyAuditLogResponse
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public EmergencyActionType Action { get; set; }
    }
}
