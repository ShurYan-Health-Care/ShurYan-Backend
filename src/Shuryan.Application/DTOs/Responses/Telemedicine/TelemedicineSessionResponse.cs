using System;

namespace Shuryan.Application.DTOs.Responses.Telemedicine
{
    public class TelemedicineSessionResponse
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string RoomId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDoctorConnected { get; set; }
        public bool IsPatientConnected { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? EndReason { get; set; }
    }
}
