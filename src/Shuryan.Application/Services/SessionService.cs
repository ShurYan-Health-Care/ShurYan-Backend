using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Responses.Session;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Enums.Notifications;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionService> _logger;
        private readonly INotificationService _notificationService;

        public SessionService(
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SessionService> logger,
            INotificationService notificationService)
        {
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<SessionResponse> StartSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Starting session for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException($"الموعد غير موجود");
                }

                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to start session for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                if (appointment.ActualStartTime.HasValue && appointment.Status == AppointmentStatus.InProgress)
                {
                    _logger.LogInformation("Active session already exists for appointment {AppointmentId}, returning existing session", appointmentId);
                    return BuildSessionResponse(appointment);
                }

                if (appointment.Status != AppointmentStatus.Confirmed)
                {
                    _logger.LogWarning("Cannot start session for appointment {AppointmentId} with status {Status}", 
                        appointmentId, appointment.Status);
                    throw new InvalidOperationException($"لا يمكن بدء جلسة لموعد بحالة {appointment.Status}");
                }

                var doctorActiveAppointment = await _appointmentRepository.GetDoctorActiveAppointmentAsync(doctorId, appointmentId);
                
                if (doctorActiveAppointment != null)
                {
                    _logger.LogWarning("Doctor {DoctorId} already has an active session with appointment {ActiveAppointmentId}", 
                        doctorId, doctorActiveAppointment.Id);
                    throw new InvalidOperationException("لديك جلسة نشطة أخرى. يرجى إنهاءها أولاً");
                }

                // ==================== Start Session ====================

                appointment.ActualStartTime = DateTime.UtcNow;
                appointment.Status = AppointmentStatus.InProgress;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Session started successfully for Appointment {AppointmentId}", appointmentId);

                // ==================== Prepare Response ====================

                return BuildSessionResponse(appointment);
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error starting session for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<SessionResponse?> GetActiveSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting active session for Appointment {AppointmentId}", appointmentId);

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    return null;
                }

                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to access session for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // التحقق من وجود جلسة (نشطة أو منتهية)
                if (!appointment.ActualStartTime.HasValue || 
                    (appointment.Status != AppointmentStatus.InProgress && appointment.Status != AppointmentStatus.Completed))
                {
                    _logger.LogInformation("No session found for Appointment {AppointmentId}", appointmentId);
                    return null;
                }

                return BuildSessionResponse(appointment);
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error getting active session for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<EndSessionResponse> EndSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Ending session for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException("الموعد غير موجود");
                }

                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to end session for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                if (!appointment.ActualStartTime.HasValue || appointment.Status != AppointmentStatus.InProgress)
                {
                    _logger.LogWarning("No active session found for Appointment {AppointmentId}", appointmentId);
                    throw new InvalidOperationException("لا توجد جلسة نشطة لهذا الموعد");
                }

                // ==================== End Session ====================

                appointment.ActualEndTime = DateTime.UtcNow;
                appointment.Status = AppointmentStatus.Completed;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Session ended successfully for Appointment {AppointmentId}", appointmentId);

                // ==================== إرسال إشعار للمريض ====================

                try
                {
                    var doctorName = appointment.Doctor != null 
                        ? $"د. {appointment.Doctor.FirstName} {appointment.Doctor.LastName}"
                        : "الدكتور";

                    await _notificationService.SendNotificationAsync(
                        userId: appointment.PatientId,
                        type: NotificationType.AppointmentCompleted,
                        title: "انتهت جلستك",
                        message: $"تم إنهاء جلستك مع {doctorName} بنجاح",
                        relatedEntityId: appointment.Id,
                        relatedEntityType: "Appointment",
                        priority: NotificationPriority.Normal
                    );

                    _logger.LogInformation(
                        "Notification sent to Patient {PatientId} for completed Appointment {AppointmentId}",
                        appointment.PatientId, appointmentId);
                }
                catch (Exception notifEx)
                {
                    // فشل الإشعار ما يمنعش إكمال العملية
                    _logger.LogError(notifEx, 
                        "Failed to send notification for completed Appointment {AppointmentId}", 
                        appointmentId);
                }

                return new EndSessionResponse
                {
                    SessionId = appointment.Id,
                    EndTime = appointment.ActualEndTime,
                    Status = appointment.Status.ToString()
                };
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error ending session for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        public async Task<SessionResponse?> GetDoctorCurrentActiveSessionAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting current active session for Doctor {DoctorId}", doctorId);

                var activeAppointment = await _appointmentRepository.GetDoctorActiveAppointmentAsync(doctorId);
                
                if (activeAppointment == null)
                {
                    _logger.LogInformation("No active session found for Doctor {DoctorId}", doctorId);
                    return null;
                }

                _logger.LogInformation("Active session found for Doctor {DoctorId} with Appointment {AppointmentId}", 
                    doctorId, activeAppointment.Id);

                return BuildSessionResponse(activeAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current active session for Doctor {DoctorId}", doctorId);
                throw;
            }
        }

        private SessionResponse BuildSessionResponse(Appointment appointment)
        {
            return new SessionResponse
            {
                SessionId = appointment.Id,
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient != null ? $"{appointment.Patient.FirstName} {appointment.Patient.LastName}" : null,
                PatientPhone = appointment.Patient?.PhoneNumber,
                PatientAge = appointment.Patient?.BirthDate.HasValue == true ? CalculateAge(appointment.Patient.BirthDate.Value) : null,
                PatientProfileImageUrl = appointment.Patient?.ProfileImageUrl,
                StartTime = appointment.ActualStartTime!.Value,
                EndTime = appointment.ActualEndTime,
                Duration = appointment.SessionDurationMinutes,
                SessionType = (int)appointment.ConsultationType,
                Status = appointment.Status.ToString(),
                ScheduledStartTime = appointment.ScheduledStartTime,
                ScheduledEndTime = appointment.ScheduledEndTime
            };
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age;
        }
    }
}
