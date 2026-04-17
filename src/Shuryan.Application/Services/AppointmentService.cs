using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Common.Pagination;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorConsultationRepository _doctorConsultationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;
        private readonly INotificationService _notificationService;
        private readonly INotificationHubService _notificationHubService;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IDoctorConsultationRepository doctorConsultationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AppointmentService> logger,
            INotificationService notificationService,
            INotificationHubService notificationHubService)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _doctorConsultationRepository = doctorConsultationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _notificationHubService = notificationHubService;
        }

        #region Basic CRUD Operations
        public async Task<AppointmentResponse?> GetAppointmentByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving appointment with ID: {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment with ID {AppointmentId} not found", id);
                    return null;
                }

                return _mapper.Map<AppointmentResponse>(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment with ID {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating appointment for Patient {PatientId} with Doctor {DoctorId}", request.PatientId, request.DoctorId);

                // 1. Validate patient exists
                var patient = await _patientRepository.GetByIdAsync(request.PatientId);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", request.PatientId);
                    throw new ArgumentException($"Patient with ID {request.PatientId} does not exist");
                }

                // 2. Validate doctor exists
                var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
                if (doctor == null)
                {
                    _logger.LogWarning("Doctor with ID {DoctorId} not found", request.DoctorId);
                    throw new ArgumentException($"Doctor with ID {request.DoctorId} does not exist");
                }

                // 3. Validate time slot
                if (request.ScheduledStartTime >= request.ScheduledEndTime)
                {
                    throw new ArgumentException("Scheduled end time must be after start time");
                }

                // تحويل الأوقات من Local Time (Egypt) لـ UTC
                // الـ client بيبعت الأوقات بتوقيت مصر، لازم نحولهم لـ UTC قبل الحفظ
                var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                
                DateTime scheduledStartTimeUtc;
                DateTime scheduledEndTimeUtc;
                
                // تحقق لو الـ DateTime جاي من الـ client بـ DateTimeKind.Unspecified (يعني local time)
                if (request.ScheduledStartTime.Kind == DateTimeKind.Unspecified)
                {
                    // نعتبره Egypt time ونحوله لـ UTC
                    scheduledStartTimeUtc = TimeZoneInfo.ConvertTimeToUtc(request.ScheduledStartTime, egyptTimeZone);
                    scheduledEndTimeUtc = TimeZoneInfo.ConvertTimeToUtc(request.ScheduledEndTime, egyptTimeZone);
                }
                else if (request.ScheduledStartTime.Kind == DateTimeKind.Local)
                {
                    // لو جاي كـ Local، نحوله لـ UTC
                    scheduledStartTimeUtc = request.ScheduledStartTime.ToUniversalTime();
                    scheduledEndTimeUtc = request.ScheduledEndTime.ToUniversalTime();
                }
                else
                {
                    // لو جاي كـ UTC، نستخدمه زي ما هو
                    scheduledStartTimeUtc = request.ScheduledStartTime;
                    scheduledEndTimeUtc = request.ScheduledEndTime;
                }

                _logger.LogInformation("Appointment times - Local: {LocalStart} to {LocalEnd}, UTC: {UtcStart} to {UtcEnd}",
                    request.ScheduledStartTime, request.ScheduledEndTime, scheduledStartTimeUtc, scheduledEndTimeUtc);

                // 4. Validate appointment is in the future
                if (scheduledStartTimeUtc <= DateTime.UtcNow)
                {
                    throw new ArgumentException("Appointment must be scheduled for a future date and time");
                }

                // 5. Check for conflicting appointments
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    request.DoctorId, 
                    scheduledStartTimeUtc, 
                    scheduledEndTimeUtc);

                if (hasConflict)
                {
                    _logger.LogWarning("Time slot conflict for Doctor {DoctorId} at {StartTime}", 
                        request.DoctorId, request.ScheduledStartTime);
                    throw new InvalidOperationException("The selected time slot is not available. Please choose another time.");
                }

                // 6. Get consultation fee from doctor's consultation types
                var doctorConsultations = await _doctorConsultationRepository.GetByDoctorIdAsync(request.DoctorId);
                var doctorConsultationsList = doctorConsultations.ToList();
                var doctorConsultation = doctorConsultationsList.FirstOrDefault(dc => 
                    dc.ConsultationType?.ConsultationTypeEnum == request.ConsultationType);

                if (doctorConsultation == null)
                {
                    throw new ArgumentException($"Doctor does not offer {request.ConsultationType} consultation type");
                }

                // ==================== Create Appointment ====================

                var appointment = new Appointment
                {
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    ScheduledStartTime = scheduledStartTimeUtc,
                    ScheduledEndTime = scheduledEndTimeUtc,
                    ConsultationType = request.ConsultationType,
                    ConsultationFee = doctorConsultation.ConsultationFee,
                    SessionDurationMinutes = doctorConsultation.SessionDurationMinutes,
                    Status = AppointmentStatus.PendingPayment,
                    CreatedAt = DateTime.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} created successfully", appointment.Id);

                // Retrieve with details for response
                var createdAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id);
                return _mapper.Map<AppointmentResponse>(createdAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for Patient {PatientId}", request.PatientId);
                throw;
            }
        }

        public async Task<AppointmentResponse> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Updating appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    throw new ArgumentException($"Appointment with ID {id} not found");
                }

                // Only allow updates for confirmed appointments
                if (appointment.Status != AppointmentStatus.Confirmed)
                {
                    throw new InvalidOperationException($"Cannot update appointment with status {appointment.Status}");
                }

                // Validate new time slot if changed
                if (request.ScheduledStartTime.HasValue && request.ScheduledEndTime.HasValue)
                {
                    if (request.ScheduledStartTime.Value >= request.ScheduledEndTime.Value)
                    {
                        throw new ArgumentException("Scheduled end time must be after start time");
                    }

                    if (request.ScheduledStartTime.Value <= DateTime.UtcNow)
                    {
                        throw new ArgumentException("Appointment must be scheduled for a future date and time");
                    }

                    // Check for conflicts (excluding current appointment)
                    var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                        appointment.DoctorId,
                        request.ScheduledStartTime.Value,
                        request.ScheduledEndTime.Value,
                        id);

                    if (hasConflict)
                    {
                        throw new InvalidOperationException("The selected time slot is not available");
                    }

                    appointment.ScheduledStartTime = request.ScheduledStartTime.Value;
                    appointment.ScheduledEndTime = request.ScheduledEndTime.Value;
                    appointment.SessionDurationMinutes = (int)(request.ScheduledEndTime.Value - request.ScheduledStartTime.Value).TotalMinutes;
                }

                appointment.UpdatedAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} updated successfully", id);

                var updatedAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<AppointmentResponse>(updatedAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAppointmentAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", id);
                    return false;
                }

                // Soft delete by marking as cancelled
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = "Deleted by user";
                appointment.CancelledAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
                throw;
            }
        }
        #endregion

        #region Appointment Management
        public async Task<AppointmentResponse> CancelAppointmentAsync(Guid id, CancelAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Cancelling appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    throw new ArgumentException($"Appointment with ID {id} not found");
                }

                // Validate appointment can be cancelled
                if (appointment.Status == AppointmentStatus.Completed)
                {
                    throw new InvalidOperationException("Cannot cancel a completed appointment");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Appointment is already cancelled");
                }

                // Check cancellation policy (must cancel at least 24 hours before)
                var hoursUntilAppointment = (appointment.ScheduledStartTime - DateTime.UtcNow).TotalHours;
                if (hoursUntilAppointment < 24)
                {
                    _logger.LogWarning("Late cancellation for appointment {AppointmentId} - only {Hours} hours notice", 
                        id, hoursUntilAppointment);
                    // i need to apply a cancellation fee here in the future too :)
                }

                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = request.CancellationReason;
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} cancelled successfully", id);

                var cancelledAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<AppointmentResponse>(cancelledAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentResponse> RescheduleAppointmentAsync(Guid id, RescheduleAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Rescheduling appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    throw new ArgumentException($"Appointment with ID {id} not found");
                }

                // Validate appointment can be rescheduled
                if (appointment.Status == AppointmentStatus.Completed)
                {
                    throw new InvalidOperationException("Cannot reschedule a completed appointment");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot reschedule a cancelled appointment");
                }

                // Validate new time slot
                if (request.NewScheduledStartTime >= request.NewScheduledEndTime)
                {
                    throw new ArgumentException("New end time must be after start time");
                }

                if (request.NewScheduledStartTime <= DateTime.UtcNow)
                {
                    throw new ArgumentException("Appointment must be rescheduled for a future date and time");
                }

                // Check for conflicts with new time slot
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    appointment.DoctorId,
                    request.NewScheduledStartTime,
                    request.NewScheduledEndTime,
                    id);

                if (hasConflict)
                {
                    throw new InvalidOperationException("The new time slot is not available");
                }

                appointment.ScheduledStartTime = request.NewScheduledStartTime;
                appointment.ScheduledEndTime = request.NewScheduledEndTime;
                appointment.SessionDurationMinutes = (int)(request.NewScheduledEndTime - request.NewScheduledStartTime).TotalMinutes;
                appointment.UpdatedAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} rescheduled successfully", id);

                var rescheduledAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<AppointmentResponse>(rescheduledAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling appointment {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentResponse> ConfirmAppointmentAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Confirming appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    throw new ArgumentException($"Appointment with ID {id} not found");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot confirm a cancelled appointment");
                }

                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} confirmed successfully", id);

                var confirmedAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<AppointmentResponse>(confirmedAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming appointment {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentResponse> CompleteAppointmentAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Completing appointment {AppointmentId}", id);

                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    throw new ArgumentException($"Appointment with ID {id} not found");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Cannot complete a cancelled appointment");
                }

                if (appointment.Status == AppointmentStatus.Completed)
                {
                    throw new InvalidOperationException("Appointment is already completed");
                }

                appointment.Status = AppointmentStatus.Completed;
                appointment.UpdatedAt = DateTime.UtcNow;

                _appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} completed successfully", id);

                var completedAppointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                return _mapper.Map<AppointmentResponse>(completedAppointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing appointment {AppointmentId}", id);
                throw;
            }
        }
        #endregion

        #region Query Operations
        /// <summary>
        /// Get all appointments for a specific patient
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsByPatientIdAsync(Guid patientId)
        {
            try
            {
                _logger.LogInformation("Retrieving appointments for Patient {PatientId}", patientId);

                var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for Patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// Get all appointments for a specific doctor
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsByDoctorIdAsync(Guid doctorId)
        {
            try
            {
                _logger.LogInformation("Retrieving appointments for Doctor {DoctorId}", doctorId);

                var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for Doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// Get upcoming appointments for a user
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetUpcomingAppointmentsAsync(Guid userId, string userRole)
        {
            try
            {
                _logger.LogInformation("Retrieving upcoming appointments for User {UserId} with role {Role}", 
                    userId, userRole);

                var isDoctor = userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
                var appointments = await _appointmentRepository.GetUpcomingAppointmentsAsync(userId, isDoctor);
                
                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming appointments for User {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get past appointments for a user
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetPastAppointmentsAsync(Guid userId, string userRole)
        {
            try
            {
                _logger.LogInformation("Retrieving past appointments for User {UserId} with role {Role}", 
                    userId, userRole);

                var isDoctor = userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
                var appointments = await _appointmentRepository.GetPastAppointmentsAsync(userId, isDoctor);
                
                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving past appointments for User {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get appointments by status
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsByStatusAsync(AppointmentStatus status)
        {
            try
            {
                _logger.LogInformation("Retrieving appointments with status {Status}", status);

                var appointments = await _appointmentRepository.GetByStatusAsync(status);
                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments with status {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get appointments within a date range
        /// </summary>
        public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Retrieving appointments between {StartDate} and {EndDate}", 
                    startDate, endDate);

                if (startDate >= endDate)
                {
                    throw new ArgumentException("End date must be after start date");
                }

                var appointments = await _appointmentRepository.FindAsync(a => 
                    a.ScheduledStartTime >= startDate && 
                    a.ScheduledStartTime <= endDate &&
                    a.Status != AppointmentStatus.Cancelled);

                return _mapper.Map<IEnumerable<AppointmentResponse>>(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments by date range");
                throw;
            }
        }

        /// <summary>
        /// Check if a time slot is available for a doctor
        /// </summary>
        public async Task<bool> IsTimeSlotAvailableAsync(Guid doctorId, DateTime startTime, DateTime endTime)
        {
            try
            {
                _logger.LogInformation("Checking time slot availability for Doctor {DoctorId} at {StartTime}", 
                    doctorId, startTime);

                if (startTime >= endTime)
                {
                    throw new ArgumentException("End time must be after start time");
                }

                if (startTime <= DateTime.UtcNow)
                {
                    return false; // Cannot book in the past
                }

                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    doctorId, startTime, endTime);

                return !hasConflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking time slot availability for Doctor {DoctorId}", doctorId);
                throw;
            }
        }

        /// <summary>
        /// Get total count of appointments for a user
        /// </summary>
        public async Task<int> GetAppointmentsCountAsync(Guid userId, string userRole)
        {
            try
            {
                _logger.LogInformation("Getting appointment count for User {UserId} with role {Role}", 
                    userId, userRole);

                var isDoctor = userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
                
                IEnumerable<Appointment> appointments;
                if (isDoctor)
                {
                    appointments = await _appointmentRepository.GetByDoctorIdAsync(userId);
                }
                else
                {
                    appointments = await _appointmentRepository.GetByPatientIdAsync(userId);
                }

                return appointments.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment count for User {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Booking System
        /// <summary>
        /// Get appointments already booked for a specific day
        /// </summary>
        public async Task<IEnumerable<BookedAppointmentSlotResponse>> GetBookedAppointmentsForDateAsync(Guid doctorId, DateTime date)
        {
            try
            {
                _logger.LogInformation("Getting booked appointments for doctor {DoctorId} on date {Date}", doctorId, date.ToString("yyyy-MM-dd"));

                // Verify doctor exists
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get start and end of the day
                var startOfDay = date.Date;
                var endOfDay = date.Date.AddDays(1).AddSeconds(-1);

                var bookedAppointments = await _appointmentRepository.GetBookedAppointmentsForDateAsync(doctorId, startOfDay, endOfDay);

                var response = bookedAppointments.Select(a => new BookedAppointmentSlotResponse
                {
                    AppointmentId = a.Id,
                    Time = a.ScheduledStartTime.ToString("HH:mm"),
                    PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : null,
                    ConsultationType = a.ConsultationType == ConsultationTypeEnum.Regular ? 0 : 1
                }).ToList();

                _logger.LogInformation("Found {Count} booked appointments for doctor {DoctorId} on {Date}",
                    response.Count, doctorId, date.ToString("yyyy-MM-dd"));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booked appointments for doctor {DoctorId} on date {Date}", doctorId, date);
                throw;
            }
        }

        /// <summary>
        /// Book a new appointment
        /// </summary>
        public async Task<BookedAppointmentResponse> BookAppointmentAsync(Guid patientId, BookAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Booking appointment for Patient {PatientId} with Doctor {DoctorId} on {Date} at {Time}",
                    patientId, request.DoctorId, request.AppointmentDate, request.AppointmentTime);

                // 1. Validate patient exists
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                    throw new ArgumentException($"Patient with ID {patientId} not found");

                // 2. Validate doctor exists
                var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {request.DoctorId} not found");

                // 3. Parse date and time
                if (!DateTime.TryParse(request.AppointmentDate, out var appointmentDate))
                    throw new ArgumentException("Invalid appointment date format. Expected YYYY-MM-DD");

                var timeParts = request.AppointmentTime.Split(':');
                if (timeParts.Length != 2 || 
                    !int.TryParse(timeParts[0], out var hour) || 
                    !int.TryParse(timeParts[1], out var minute))
                    throw new ArgumentException("Invalid appointment time format. Expected HH:mm");

                var scheduledStartTime = new DateTime(appointmentDate.Year, appointmentDate.Month, appointmentDate.Day, hour, minute, 0);

                // 4. Validate date is not in the past
                if (scheduledStartTime < DateTime.Now)
                    throw new InvalidOperationException("You cannot book an appointment in the past.");

                // 5. Validate consultationType (1 or 2)
                if (request.ConsultationType != 1 && request.ConsultationType != 2)
                    throw new ArgumentException("Consultation type is incorrect. Must be 1 (Regular) or 2 (FollowUp)");

                var consultationType = request.ConsultationType == 1 ? ConsultationTypeEnum.Regular : ConsultationTypeEnum.FollowUp;

                // 6. Get consultation pricing and duration
                var consultationTypeEntity = await _unitOfWork.ConsultationTypes.GetByEnumAsync(consultationType);

                if (consultationTypeEntity == null)
                    throw new InvalidOperationException("Consultation type not found in system");

                var doctorConsultation = await _doctorConsultationRepository.GetByDoctorIdAndConsultationTypeIdAsync(request.DoctorId, consultationTypeEntity.Id);

                if (doctorConsultation == null)
                    throw new InvalidOperationException("Doctor has not set price or duration for this consultation type");

                var consultationFee = doctorConsultation.ConsultationFee;
                var durationMinutes = doctorConsultation.SessionDurationMinutes;
                var scheduledEndTime = scheduledStartTime.AddMinutes(durationMinutes);

                // 7. Check if time slot is available
                var isAvailable = await IsTimeSlotAvailableAsync(request.DoctorId, scheduledStartTime, scheduledEndTime);
                if (!isAvailable)
                    throw new InvalidOperationException($"The time slot {request.AppointmentTime} is already booked");

                // ==================== Create Appointment ====================

                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    DoctorId = request.DoctorId,
                    ScheduledStartTime = scheduledStartTime,
                    ScheduledEndTime = scheduledEndTime,
                    ConsultationType = consultationType,
                    ConsultationFee = consultationFee,
                    SessionDurationMinutes = durationMinutes,
                    Status = AppointmentStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully booked appointment {AppointmentId} for Patient {PatientId}",
                    appointment.Id, patientId);

                // ==================== Send Real-time Appointment Data to Doctor ====================
                // بنبعت الـ appointment data كاملة للدكتور عبر SignalR بس لو الحجز في نفس اليوم
                try
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var todayInEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone).Date;
                    var appointmentDateInEgypt = TimeZoneInfo.ConvertTimeFromUtc(scheduledStartTime, egyptTimeZone).Date;

                    // لو الحجز في نفس اليوم، ابعت الـ appointment data للدكتور
                    if (appointmentDateInEgypt == todayInEgypt)
                    {
                        // بناء الـ appointment DTO للإرسال
                        var appointmentDto = new DTOs.Responses.Appointment.DoctorAppointmentResponse
                        {
                            Id = appointment.Id,
                            PatientId = appointment.PatientId,
                            PatientName = $"{patient.FirstName} {patient.LastName}",
                            PatientPhoneNumber = patient.PhoneNumber,
                            AppointmentDate = appointmentDate.ToString("yyyy-MM-dd"),
                            AppointmentTime = request.AppointmentTime,
                            Duration = durationMinutes,
                            AppointmentType = consultationType == ConsultationTypeEnum.FollowUp ? "followup" : "regular",
                            Status = appointment.Status,
                            CreatedAt = appointment.CreatedAt,
                            Notes = null,
                            Price = appointment.ConsultationFee
                        };

                        // إرسال عبر SignalR Hub
                        await _notificationHubService.SendNotificationToUserAsync(
                            userId: request.DoctorId,
                            title: "NewAppointmentToday", // Event name للـ Frontend
                            message: $"حجز جديد من {patient.FirstName} {patient.LastName}",
                            data: appointmentDto
                        );

                        // كمان نحفظ notification في الـ Database (للـ persistence)
                        await _notificationService.SendNotificationAsync(
                            userId: request.DoctorId,
                            type: Core.Enums.Notifications.NotificationType.AppointmentConfirmed,
                            title: "حجز جديد اليوم",
                            message: $"المريض {patient.FirstName} {patient.LastName} حجز معاك موعد النهاردة الساعة {request.AppointmentTime}",
                            relatedEntityId: appointment.Id,
                            relatedEntityType: "Appointment",
                            priority: Core.Enums.Notifications.NotificationPriority.High
                        );

                        _logger.LogInformation(
                            "Sent real-time appointment data to Doctor {DoctorId} for same-day appointment {AppointmentId}",
                            request.DoctorId, appointment.Id);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Skipped real-time update for Doctor {DoctorId} - appointment {AppointmentId} is not today",
                            request.DoctorId, appointment.Id);
                    }
                }
                catch (Exception signalREx)
                {
                    // لو في مشكلة في إرسال الـ SignalR، ما نخليش ده يأثر على إنشاء الحجز
                    _logger.LogError(signalREx, 
                        "Failed to send real-time update to Doctor {DoctorId} for appointment {AppointmentId}. Appointment was created successfully.",
                        request.DoctorId, appointment.Id);
                }

                // Return response
                return new BookedAppointmentResponse
                {
                    Id = appointment.Id,
                    DoctorId = appointment.DoctorId,
                    PatientId = appointment.PatientId,
                    AppointmentDate = appointmentDate.ToString("yyyy-MM-dd"),
                    AppointmentTime = request.AppointmentTime,
                    ConsultationType = request.ConsultationType,
                    Status = appointment.Status.ToString(),
                    TotalAmount = appointment.ConsultationFee,
                    CreatedAt = appointment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment for Patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<AvailableTimeSlotResponse>> GetAvailableTimeSlotsAsync(Guid doctorId, DateTime date, int consultationType)
        {
            try
            {
                _logger.LogInformation("Calculating available time slots for doctor {DoctorId} on {Date} for consultationType {ConsultationType}",
                    doctorId, date.ToString("yyyy-MM-dd"), consultationType);

                var slots = new List<AvailableTimeSlotResponse>();

                // Validate doctor exists
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {doctorId} not found");

                // Get duration for this consultation type
                var consultationTypeEnum = consultationType == 1 ? ConsultationTypeEnum.Regular : ConsultationTypeEnum.FollowUp;
                var consultationTypeEntity = await _unitOfWork.ConsultationTypes.GetByEnumAsync(consultationTypeEnum);

                if (consultationTypeEntity == null)
                    return slots; // Return empty list

                var doctorConsultation = await _doctorConsultationRepository
                    .GetByDoctorIdAndConsultationTypeIdAsync(doctorId, consultationTypeEntity.Id);

                if (doctorConsultation == null)
                    return slots; // Return empty list

                var durationMinutes = doctorConsultation.SessionDurationMinutes;

                // TODO: Get schedule (weekly or override) for this date
                // TODO: Generate time slots based on schedule
                // TODO: Get booked appointments for this date
                // TODO: Mark slots as available/booked/past

                // For now, return empty list - this is optional endpoint
                _logger.LogInformation("Available slots calculation not fully implemented yet (optional feature)");

                return slots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating available time slots for doctor {DoctorId}", doctorId);
                throw;
            }
        }
        #endregion

        #region Doctor Appointments Management
        /// <summary>
        /// Get doctor appointments with pagination and filters
        /// </summary>
        public async Task<PaginatedResponse<DoctorAppointmentResponse>> GetDoctorAppointmentsAsync(
            Guid doctorId, 
            GetDoctorAppointmentsRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Retrieving appointments for Doctor {DoctorId} - Page: {Page}, Size: {Size}, StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}",
                    doctorId, request.PageNumber, request.PageSize, request.StartDate, request.EndDate, request.Status);

                // Get appointments from repository with filters and pagination
                var (appointments, totalCount) = await _appointmentRepository.GetByDoctorIdWithFiltersAsync(
                    doctorId,
                    request.StartDate,
                    request.EndDate,
                    request.Status,
                    request.PageNumber,
                    request.PageSize,
                    request.SortBy,
                    request.SortOrder);

                // Get statistics for ALL appointments (not just current page)
                var statisticsDict = await _appointmentRepository.GetAppointmentStatisticsByDoctorIdAsync(
                    doctorId,
                    request.StartDate,
                    request.EndDate);

                // Map to response DTOs
                var appointmentResponses = appointments.Select(a => new DoctorAppointmentResponse
                {
                    Id = a.Id,
                    PatientId = a.PatientId,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PatientPhoneNumber = a.Patient.PhoneNumber,
                    AppointmentDate = a.ScheduledStartTime.ToString("yyyy-MM-dd"),
                    AppointmentTime = a.ScheduledStartTime.ToString("HH:mm:ss"),
                    Duration = a.SessionDurationMinutes,
                    AppointmentType = a.ConsultationType == ConsultationTypeEnum.FollowUp ? "followup" : "regular",
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    Notes = a.CancellationReason,
                    Price = a.ConsultationFee
                }).ToList();

                // Build statistics object
                var statistics = new AppointmentStatistics
                {
                    Total = totalCount,
                    Pending = 0, // Pending is not a status in the enum
                    Confirmed = statisticsDict.GetValueOrDefault(AppointmentStatus.Confirmed, 0),
                    CheckedIn = statisticsDict.GetValueOrDefault(AppointmentStatus.CheckedIn, 0),
                    InProgress = statisticsDict.GetValueOrDefault(AppointmentStatus.InProgress, 0),
                    Completed = statisticsDict.GetValueOrDefault(AppointmentStatus.Completed, 0),
                    NoShow = statisticsDict.GetValueOrDefault(AppointmentStatus.NoShow, 0),
                    Cancelled = statisticsDict.GetValueOrDefault(AppointmentStatus.Cancelled, 0)
                };

                // Build paginated response
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
                
                var paginatedResponse = new PaginatedResponse<DoctorAppointmentResponse>
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages,
                    Data = appointmentResponses,
                    Statistics = statistics
                };

                _logger.LogInformation(
                    "Retrieved {Count} appointments out of {TotalCount} for Doctor {DoctorId}",
                    appointmentResponses.Count, totalCount, doctorId);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for Doctor {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion
    }
}
