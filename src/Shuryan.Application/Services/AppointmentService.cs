using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Appointment;
using Shuryan.Application.DTOs.Responses.Appointment;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical.Appointments;
using Shuryan.Core.Enums.Appointments;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    /// <summary>
    /// Service for managing patient appointments with comprehensive business logic
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorConsultationRepository _doctorConsultationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IDoctorConsultationRepository doctorConsultationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _doctorConsultationRepository = doctorConsultationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Get appointment by ID with full details
        /// </summary>
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

        /// <summary>
        /// Create a new appointment with comprehensive validation
        /// </summary>
        public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating appointment for Patient {PatientId} with Doctor {DoctorId}", 
                    request.PatientId, request.DoctorId);

                // ==================== Validation ====================

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

                // 4. Validate appointment is in the future
                if (request.ScheduledStartTime <= DateTime.UtcNow)
                {
                    throw new ArgumentException("Appointment must be scheduled for a future date and time");
                }

                // 5. Check for conflicting appointments
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    request.DoctorId, 
                    request.ScheduledStartTime, 
                    request.ScheduledEndTime);

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
                    ScheduledStartTime = request.ScheduledStartTime,
                    ScheduledEndTime = request.ScheduledEndTime,
                    ConsultationType = request.ConsultationType,
                    ConsultationFee = doctorConsultation.ConsultationFee,
                    SessionDurationMinutes = doctorConsultation.SessionDurationMinutes,
                    Status = AppointmentStatus.Confirmed,
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

        /// <summary>
        /// Update appointment details (limited fields)
        /// </summary>
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

        /// <summary>
        /// Soft delete appointment (mark as cancelled)
        /// </summary>
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

        /// <summary>
        /// Cancel an appointment with reason
        /// </summary>
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

                // Check cancellation policy (e.g., must cancel at least 24 hours before)
                var hoursUntilAppointment = (appointment.ScheduledStartTime - DateTime.UtcNow).TotalHours;
                if (hoursUntilAppointment < 24)
                {
                    _logger.LogWarning("Late cancellation for appointment {AppointmentId} - only {Hours} hours notice", 
                        id, hoursUntilAppointment);
                    // You might want to apply a cancellation fee here
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

        /// <summary>
        /// Reschedule an appointment to a new time
        /// </summary>
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

        /// <summary>
        /// Confirm an appointment (typically done by doctor or clinic staff)
        /// </summary>
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

        /// <summary>
        /// Mark appointment as completed
        /// </summary>
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
        /// Get upcoming appointments for a user (patient or doctor)
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
        /// Get past appointments for a user (patient or doctor)
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
    }
}
