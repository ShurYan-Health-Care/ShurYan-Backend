using System;
using System.Threading.Tasks;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Telemedicine;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Medical;

namespace Shuryan.Application.Interfaces
{
    public interface ICallSessionService
    {
        // ==================== REST API Operations ====================

        Task<ApiResponse<TelemedicineSessionResponse>> CreateSessionAsync(Guid appointmentId, Guid requestingDoctorId);

        Task<ApiResponse<TelemedicineSessionResponse>> GetSessionByAppointmentIdAsync(Guid appointmentId, Guid requestingUserId);

        IceConfigResponse GetIceConfig();

        // ==================== Hub-Internal Operations ====================
        // These are called by CallHub directly, not exposed via REST.
        // They return plain objects — the hub handles its own client error signaling.

        Task<TelemedicineSession?> GetSessionByRoomIdAsync(string roomId);

        Task<bool> DoctorJoinedAsync(string roomId, Guid doctorId, string connectionId);

        Task<bool> PatientJoinedAsync(string roomId, Guid patientId, string connectionId);

        Task EndSessionAsync(string roomId, SessionEndReason reason);

        Task HandleDisconnectAsync(string connectionId);
    }
}
