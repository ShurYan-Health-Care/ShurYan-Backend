using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Responses.Session;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    /// <summary>
    /// Service مسؤول عن إدارة جلسات الكشف (Consultation Sessions)
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SessionService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// بدء جلسة كشف جديدة
        /// </summary>
        public async Task<SessionResponse> StartSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Starting session for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                // 1. التحقق من وجود الموعد
                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException($"الموعد غير موجود");
                }

                // 2. التحقق من أن الموعد يخص الدكتور المسجل دخوله
                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to start session for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // 3. التحقق من وجود جلسة نشطة للموعد - Return existing session بدلاً من error
                if (appointment.ActualStartTime.HasValue && appointment.Status == AppointmentStatus.InProgress)
                {
                    _logger.LogInformation("Active session already exists for appointment {AppointmentId}, returning existing session", appointmentId);
                    return BuildSessionResponse(appointment);
                }

                // 4. التحقق من حالة الموعد (يجب أن يكون Confirmed)
                if (appointment.Status != AppointmentStatus.Confirmed)
                {
                    _logger.LogWarning("Cannot start session for appointment {AppointmentId} with status {Status}", 
                        appointmentId, appointment.Status);
                    throw new InvalidOperationException($"لا يمكن بدء جلسة لموعد بحالة {appointment.Status}");
                }

                // 5. التحقق من عدم وجود جلسة نشطة أخرى للدكتور (Performance Optimized)
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

        /// <summary>
        /// الحصول على الجلسة النشطة للموعد
        /// </summary>
        public async Task<SessionResponse?> GetActiveSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting active session for Appointment {AppointmentId}", appointmentId);

                // التحقق من وجود الموعد وأنه يخص الدكتور
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

        /// <summary>
        /// إنهاء الجلسة النشطة
        /// </summary>
        public async Task<EndSessionResponse> EndSessionAsync(Guid appointmentId, Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Ending session for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                // 1. التحقق من وجود الموعد
                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException("الموعد غير موجود");
                }

                // 2. التحقق من أن الموعد يخص الدكتور
                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to end session for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // 3. التحقق من وجود جلسة نشطة
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

        /// <summary>
        /// الحصول على الجلسة النشطة الحالية للدكتور (أي موعد)
        /// </summary>
        public async Task<SessionResponse?> GetDoctorCurrentActiveSessionAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Getting current active session for Doctor {DoctorId}", doctorId);

                // البحث عن أي موعد نشط للدكتور
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

        /// <summary>
        /// بناء SessionResponse من Appointment
        /// </summary>
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

        /// <summary>
        /// حساب العمر من تاريخ الميلاد
        /// </summary>
        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            // لو لسه ما جاش عيد ميلاده السنة دي، نطرح سنة
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age;
        }
    }
}
