using System;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.DTOs.Responses.VideoCall
{
    public class VideoSessionResponse
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string AgoraChannelName { get; set; } = string.Empty;
        public string AgoraAppId { get; set; } = string.Empty;
        public string AgoraToken { get; set; } = string.Empty;
        public VideoSessionStatus Status { get; set; }
        public DateTime? DoctorJoinedAt { get; set; }
        public DateTime? PatientJoinedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public VideoSessionEndReason? EndReason { get; set; }
        public DateTime ScheduledEndTime { get; set; }
    }
}
