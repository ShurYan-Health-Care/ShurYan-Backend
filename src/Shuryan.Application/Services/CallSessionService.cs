using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.Telemedicine;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Core.Settings;
using System.Collections.Generic;

namespace Shuryan.Application.Services
{
    public class CallSessionService : ICallSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TelemedicineSettings _settings;
        private readonly ILogger<CallSessionService> _logger;

        public CallSessionService(
            IUnitOfWork unitOfWork,
            IOptions<TelemedicineSettings> settings,
            ILogger<CallSessionService> logger)
        {
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        #region REST API Operations

        public async Task<ApiResponse<TelemedicineSessionResponse>> CreateSessionAsync(
            Guid appointmentId, Guid requestingDoctorId)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("CreateSession: appointment {AppointmentId} not found", appointmentId);
                return ApiResponse<TelemedicineSessionResponse>.Failure(
                    "Appointment not found", statusCode: 404);
            }

            if (appointment.DoctorId != requestingDoctorId)
            {
                _logger.LogWarning(
                    "CreateSession: user {UserId} is not the doctor on appointment {AppointmentId}",
                    requestingDoctorId, appointmentId);
                return ApiResponse<TelemedicineSessionResponse>.Failure(
                    "You are not authorized to create a session for this appointment", statusCode: 403);
            }

            if (appointment.Status == AppointmentStatus.Cancelled
                || appointment.Status == AppointmentStatus.Completed
                || appointment.Status == AppointmentStatus.NoShow)
            {
                _logger.LogWarning(
                    "CreateSession: appointment {AppointmentId} is in terminal status {Status}",
                    appointmentId, appointment.Status);
                return ApiResponse<TelemedicineSessionResponse>.Failure(
                    "Cannot create a session for an appointment with status: " + appointment.Status, statusCode: 400);
            }

            var existing = await _unitOfWork.TelemedicineSessions.GetByAppointmentIdAsync(appointmentId);

            if (existing != null)
            {
                if (existing.Status == TelemedicineSessionStatus.Ended
                    || existing.Status == TelemedicineSessionStatus.Failed
                    || existing.Status == TelemedicineSessionStatus.Abandoned)
                {
                    return ApiResponse<TelemedicineSessionResponse>.Failure(
                        "A telemedicine session for this appointment has already ended", statusCode: 409);
                }

                _logger.LogInformation(
                    "CreateSession: returning existing session {SessionId} for appointment {AppointmentId}",
                    existing.Id, appointmentId);
                return ApiResponse<TelemedicineSessionResponse>.Success(
                    MapToResponse(existing), "Session already exists");
            }

            var session = new TelemedicineSession
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                RoomId = $"room_{appointmentId:N}",
                Status = TelemedicineSessionStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = requestingDoctorId
            };

            await _unitOfWork.TelemedicineSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Telemedicine session {SessionId} created for appointment {AppointmentId}. RoomId: {RoomId}",
                session.Id, appointmentId, session.RoomId);

            return ApiResponse<TelemedicineSessionResponse>.Success(
                MapToResponse(session), "Session created successfully", 201);
        }

        public async Task<ApiResponse<TelemedicineSessionResponse>> GetSessionByAppointmentIdAsync(
            Guid appointmentId, Guid requestingUserId)
        {
            var session = await _unitOfWork.TelemedicineSessions.GetByAppointmentIdAsync(appointmentId);

            if (session == null)
            {
                return ApiResponse<TelemedicineSessionResponse>.Failure(
                    "No telemedicine session found for this appointment", statusCode: 404);
            }

            if (session.DoctorId != requestingUserId && session.PatientId != requestingUserId)
            {
                _logger.LogWarning(
                    "GetSession: user {UserId} is not a participant of session {SessionId}",
                    requestingUserId, session.Id);
                return ApiResponse<TelemedicineSessionResponse>.Failure(
                    "You are not a participant of this session", statusCode: 403);
            }

            return ApiResponse<TelemedicineSessionResponse>.Success(MapToResponse(session));
        }

        public IceConfigResponse GetIceConfig()
        {
            var servers = new List<IceServerConfig>();

            foreach (var stun in _settings.StunServers)
                servers.Add(new IceServerConfig { Urls = new[] { stun } });

            if (!string.IsNullOrWhiteSpace(_settings.TurnServer))
                servers.Add(new IceServerConfig
                {
                    Urls = new[] { _settings.TurnServer },
                    Username = _settings.TurnUsername,
                    Credential = _settings.TurnCredential
                });

            return new IceConfigResponse { IceServers = servers };
        }

        #endregion

        #region Hub-Internal Operations

        public async Task<TelemedicineSession?> GetSessionByRoomIdAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId)) return null;
            return await _unitOfWork.TelemedicineSessions.GetByRoomIdAsync(roomId);
        }

        public async Task<bool> DoctorJoinedAsync(string roomId, Guid doctorId, string connectionId)
        {
            var session = await _unitOfWork.TelemedicineSessions.GetByRoomIdAsync(roomId);

            if (session == null)
            {
                _logger.LogWarning("DoctorJoined: room {RoomId} not found", roomId);
                return false;
            }

            if (session.DoctorId != doctorId)
            {
                _logger.LogWarning(
                    "DoctorJoined: user {UserId} is not the doctor for room {RoomId}", doctorId, roomId);
                return false;
            }

            if (session.Status == TelemedicineSessionStatus.Ended
                || session.Status == TelemedicineSessionStatus.Failed
                || session.Status == TelemedicineSessionStatus.Abandoned)
            {
                _logger.LogWarning(
                    "DoctorJoined: session {RoomId} is already in terminal status {Status}",
                    roomId, session.Status);
                return false;
            }

            var now = DateTime.UtcNow;
            session.DoctorConnectionId = connectionId;
            session.DoctorJoinedAt = now;
            session.UpdatedAt = now;
            session.UpdatedBy = doctorId;

            if (session.PatientConnectionId != null)
            {
                session.Status = TelemedicineSessionStatus.Active;
                session.StartedAt = now;
                _logger.LogInformation("Session {RoomId} is now Active — both participants connected", roomId);
            }

            _unitOfWork.TelemedicineSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Doctor {DoctorId} joined session {RoomId} with connection {ConnectionId}",
                doctorId, roomId, connectionId);

            return true;
        }

        public async Task<bool> PatientJoinedAsync(string roomId, Guid patientId, string connectionId)
        {
            var session = await _unitOfWork.TelemedicineSessions.GetByRoomIdAsync(roomId);

            if (session == null)
            {
                _logger.LogWarning("PatientJoined: room {RoomId} not found", roomId);
                return false;
            }

            if (session.PatientId != patientId)
            {
                _logger.LogWarning(
                    "PatientJoined: user {UserId} is not the patient for room {RoomId}", patientId, roomId);
                return false;
            }

            if (session.Status == TelemedicineSessionStatus.Ended
                || session.Status == TelemedicineSessionStatus.Failed
                || session.Status == TelemedicineSessionStatus.Abandoned)
            {
                _logger.LogWarning(
                    "PatientJoined: session {RoomId} is already in terminal status {Status}",
                    roomId, session.Status);
                return false;
            }

            var now = DateTime.UtcNow;
            session.PatientConnectionId = connectionId;
            session.PatientJoinedAt = now;
            session.UpdatedAt = now;
            session.UpdatedBy = patientId;

            if (session.DoctorConnectionId != null)
            {
                session.Status = TelemedicineSessionStatus.Active;
                session.StartedAt = now;
                _logger.LogInformation("Session {RoomId} is now Active — both participants connected", roomId);
            }

            _unitOfWork.TelemedicineSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Patient {PatientId} joined session {RoomId} with connection {ConnectionId}",
                patientId, roomId, connectionId);

            return true;
        }

        public async Task EndSessionAsync(string roomId, SessionEndReason reason)
        {
            var session = await _unitOfWork.TelemedicineSessions.GetByRoomIdAsync(roomId);

            if (session == null)
            {
                _logger.LogWarning("EndSession: room {RoomId} not found", roomId);
                return;
            }

            if (session.Status == TelemedicineSessionStatus.Ended)
            {
                _logger.LogWarning("EndSession: session {RoomId} is already ended", roomId);
                return;
            }

            var now = DateTime.UtcNow;
            session.Status = TelemedicineSessionStatus.Ended;
            session.EndedAt = now;
            session.EndReason = reason;
            session.UpdatedAt = now;

            if (session.StartedAt.HasValue)
                session.DurationSeconds = (int)(now - session.StartedAt.Value).TotalSeconds;

            _unitOfWork.TelemedicineSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Session {RoomId} ended. Reason: {Reason}, Duration: {Duration}s",
                roomId, reason, session.DurationSeconds);
        }

        public async Task HandleDisconnectAsync(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return;

            var session = await _unitOfWork.TelemedicineSessions.FirstOrDefaultAsync(ts =>
                (ts.DoctorConnectionId == connectionId || ts.PatientConnectionId == connectionId)
                && (ts.Status == TelemedicineSessionStatus.Waiting
                    || ts.Status == TelemedicineSessionStatus.Active));

            if (session == null) return;

            var reason = session.DoctorConnectionId == connectionId
                ? SessionEndReason.DoctorLeft
                : SessionEndReason.PatientLeft;

            _logger.LogInformation(
                "Connection {ConnectionId} lost. Ending session {RoomId} with reason {Reason}",
                connectionId, session.RoomId, reason);

            await EndSessionAsync(session.RoomId, reason);
        }

        #endregion

        #region Private Helpers

        private static TelemedicineSessionResponse MapToResponse(TelemedicineSession session)
        {
            return new TelemedicineSessionResponse
            {
                Id = session.Id,
                AppointmentId = session.AppointmentId,
                DoctorId = session.DoctorId,
                PatientId = session.PatientId,
                RoomId = session.RoomId,
                Status = session.Status.ToString(),
                IsDoctorConnected = session.DoctorConnectionId != null,
                IsPatientConnected = session.PatientConnectionId != null,
                CreatedAt = session.CreatedAt,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                DurationSeconds = session.DurationSeconds,
                EndReason = session.EndReason?.ToString()
            };
        }

        #endregion
    }
}
