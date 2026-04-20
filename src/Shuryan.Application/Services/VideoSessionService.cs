using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuryan.Application.DTOs.Common.Base;
using Shuryan.Application.DTOs.Responses.VideoCall;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Medical;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Core.Settings;

namespace Shuryan.Application.Services
{
    public class VideoSessionService : IVideoSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAgoraTokenService _tokenService;
        private readonly IVideoNotificationHubService _hubService;
        private readonly AgoraSettings _agoraSettings;
        private readonly ILogger<VideoSessionService> _logger;

        private static readonly AppointmentStatus[] ValidJoinStatuses =
        {
            AppointmentStatus.Confirmed,
            AppointmentStatus.CheckedIn,
            AppointmentStatus.InProgress
        };

        public VideoSessionService(
            IUnitOfWork unitOfWork,
            IAgoraTokenService tokenService,
            IVideoNotificationHubService hubService,
            IOptions<AgoraSettings> agoraSettings,
            ILogger<VideoSessionService> logger)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _hubService = hubService;
            _agoraSettings = agoraSettings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<VideoSessionResponse>> JoinSessionAsync(Guid appointmentId, Guid userId)
        {
            try
            {
                var session = await _unitOfWork.VideoSessions.GetByAppointmentIdAsync(appointmentId);
                if (session is null)
                    return ApiResponse<VideoSessionResponse>.Failure("Video session not found for this appointment.", statusCode: 404);

                var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
                if (appointment is null)
                    return ApiResponse<VideoSessionResponse>.Failure("Appointment not found.", statusCode: 404);

                var isDoctor = session.DoctorId == userId;
                var isPatient = session.PatientId == userId;

                if (!isDoctor && !isPatient)
                    return ApiResponse<VideoSessionResponse>.Failure("You are not a participant in this session.", statusCode: 403);

                if (!Array.Exists(ValidJoinStatuses, s => s == appointment.Status))
                    return ApiResponse<VideoSessionResponse>.Failure(
                        $"Cannot join. Appointment status is '{appointment.Status}'.", statusCode: 400);

                if (session.Status == VideoSessionStatus.Ended || session.Status == VideoSessionStatus.Abandoned)
                    return ApiResponse<VideoSessionResponse>.Failure("This session has already ended.", statusCode: 400);

                var now = DateTime.UtcNow;

                if (isDoctor && session.DoctorJoinedAt is null)
                {
                    session.DoctorJoinedAt = now;
                    _unitOfWork.VideoSessions.Update(session);
                    _logger.LogInformation("Doctor {DoctorId} joined VideoSession for Appointment {AppointmentId}", userId, appointmentId);
                }
                else if (isPatient && session.PatientJoinedAt is null)
                {
                    session.PatientJoinedAt = now;
                    _unitOfWork.VideoSessions.Update(session);
                    _logger.LogInformation("Patient {PatientId} joined VideoSession for Appointment {AppointmentId}", userId, appointmentId);
                }

                var bothJoined = session.DoctorJoinedAt.HasValue && session.PatientJoinedAt.HasValue;

                if (bothJoined && session.StartedAt is null)
                {
                    session.StartedAt = now;
                    session.Status = VideoSessionStatus.Active;
                    _unitOfWork.VideoSessions.Update(session);
                    _logger.LogInformation("VideoSession for Appointment {AppointmentId} is now Active", appointmentId);
                }

                await _unitOfWork.SaveChangesAsync();

                if (bothJoined && session.StartedAt == now)
                    await _hubService.NotifySessionReadyAsync(session.PatientId, appointmentId, session.AgoraChannelName);

                var token = _tokenService.GenerateRtcToken(
                    session.AgoraChannelName,
                    userId.ToString("N"),
                    isPublisher: true);

                return ApiResponse<VideoSessionResponse>.Success(MapToResponse(session, token, appointment.ScheduledEndTime), "Joined successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinSessionAsync for Appointment {AppointmentId}", appointmentId);
                return ApiResponse<VideoSessionResponse>.Failure("An error occurred while joining the session.", statusCode: 500);
            }
        }

        public async Task<ApiResponse<VideoSessionResponse>> GetSessionAsync(Guid appointmentId, Guid userId)
        {
            try
            {
                var session = await _unitOfWork.VideoSessions.GetByAppointmentIdAsync(appointmentId);
                if (session is null)
                    return ApiResponse<VideoSessionResponse>.Failure("Video session not found.", statusCode: 404);

                var isDoctor = session.DoctorId == userId;
                var isPatient = session.PatientId == userId;

                if (!isDoctor && !isPatient)
                    return ApiResponse<VideoSessionResponse>.Failure("You are not a participant in this session.", statusCode: 403);

                var token = _tokenService.GenerateRtcToken(
                    session.AgoraChannelName,
                    userId.ToString("N"),
                    isPublisher: true);

                var appt = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
                var scheduledEnd = appt?.ScheduledEndTime ?? DateTime.UtcNow;

                return ApiResponse<VideoSessionResponse>.Success(MapToResponse(session, token, scheduledEnd));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSessionAsync for Appointment {AppointmentId}", appointmentId);
                return ApiResponse<VideoSessionResponse>.Failure("An error occurred while retrieving the session.", statusCode: 500);
            }
        }

        public async Task<ApiResponse<bool>> EndSessionAsync(Guid appointmentId, Guid userId, VideoSessionEndReason reason)
        {
            try
            {
                var session = await _unitOfWork.VideoSessions.GetByAppointmentIdAsync(appointmentId);
                if (session is null)
                    return ApiResponse<bool>.Failure("Video session not found.", statusCode: 404);

                if (session.DoctorId != userId && session.PatientId != userId)
                    return ApiResponse<bool>.Failure("You are not a participant in this session.", statusCode: 403);

                if (session.Status == VideoSessionStatus.Ended || session.Status == VideoSessionStatus.Abandoned)
                    return ApiResponse<bool>.Failure("Session is already ended.", statusCode: 400);

                var now = DateTime.UtcNow;
                session.EndedAt = now;
                session.EndReason = reason;
                session.Status = VideoSessionStatus.Ended;

                if (session.StartedAt.HasValue)
                    session.DurationSeconds = (int)(now - session.StartedAt.Value).TotalSeconds;

                _unitOfWork.VideoSessions.Update(session);

                var appointment = await _unitOfWork.Appointments.GetByIdAsync(session.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Completed;
                    appointment.UpdatedAt = now;
                    _unitOfWork.Appointments.Update(appointment);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "VideoSession for Appointment {AppointmentId} ended by {UserId}. Reason: {Reason}. Duration: {Seconds}s",
                    appointmentId, userId, reason, session.DurationSeconds);

                var otherParticipantId = session.DoctorId == userId ? session.PatientId : session.DoctorId;
                await _hubService.NotifySessionEndedAsync(otherParticipantId, appointmentId);

                return ApiResponse<bool>.Success(true, "Session ended successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EndSessionAsync for Appointment {AppointmentId}", appointmentId);
                return ApiResponse<bool>.Failure("An error occurred while ending the session.", statusCode: 500);
            }
        }

        private VideoSessionResponse MapToResponse(Core.Entities.Medical.VideoSession session, string token, DateTime scheduledEndTime)
        {
            return new VideoSessionResponse
            {
                Id = session.Id,
                AppointmentId = session.AppointmentId,
                DoctorId = session.DoctorId,
                PatientId = session.PatientId,
                AgoraChannelName = session.AgoraChannelName,
                AgoraAppId = _agoraSettings.AppId,
                AgoraToken = token,
                Status = session.Status,
                DoctorJoinedAt = session.DoctorJoinedAt,
                PatientJoinedAt = session.PatientJoinedAt,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                DurationSeconds = session.DurationSeconds,
                EndReason = session.EndReason,
                ScheduledEndTime = scheduledEndTime
            };
        }
    }
}
