using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shuryan.Application.DTOs.Requests.Documentation;
using Shuryan.Application.DTOs.Responses.Documentation;
using Shuryan.Application.Interfaces;
using Shuryan.Core.Entities.Medical.Consultations;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;

namespace Shuryan.Application.Services
{
    public class DocumentationService : IDocumentationService
    {
        private readonly IConsultationRecordRepository _consultationRecordRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DocumentationService> _logger;

        public DocumentationService(
            IConsultationRecordRepository consultationRecordRepository,
            IAppointmentRepository appointmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<DocumentationService> logger)
        {
            _consultationRecordRepository = consultationRecordRepository;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DocumentationResponse> SaveDocumentationAsync(
            Guid appointmentId, 
            Guid doctorId, 
            SaveDocumentationRequest request)
        {
            try
            {
                _logger.LogInformation("Saving documentation for Appointment {AppointmentId} by Doctor {DoctorId}", 
                    appointmentId, doctorId);

                // ==================== Validation ====================

                // 1. Verify the appointment exists
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    throw new ArgumentException("الموعد غير موجود");
                }

                // 2. Verify that the appointment is for the doctor.
                if (appointment.DoctorId != doctorId)
                {
                    _logger.LogWarning("Doctor {DoctorId} tried to save documentation for appointment {AppointmentId} that belongs to another doctor", 
                        doctorId, appointmentId);
                    throw new UnauthorizedAccessException("هذا الموعد لا يخصك");
                }

                // ==================== Create or Update ====================

                // Check for previous documentation
                var existingRecord = await _consultationRecordRepository.GetByAppointmentIdAsync(appointmentId);

                if (existingRecord != null)
                {
                    // Update existing record
                    _logger.LogInformation("Updating existing consultation record {RecordId}", existingRecord.Id);

                    // Update only non-null fields (partial save support)
                    if (request.ChiefComplaint != null)
                        existingRecord.ChiefComplaint = request.ChiefComplaint;
                    
                    if (request.HistoryOfPresentIllness != null)
                        existingRecord.HistoryOfPresentIllness = request.HistoryOfPresentIllness;
                    
                    if (request.PhysicalExamination != null)
                        existingRecord.PhysicalExamination = request.PhysicalExamination;
                    
                    if (request.Diagnosis != null)
                        existingRecord.Diagnosis = request.Diagnosis;
                    
                    if (request.ManagementPlan != null)
                        existingRecord.ManagementPlan = request.ManagementPlan;

                    _consultationRecordRepository.Update(existingRecord);
                    await _unitOfWork.SaveChangesAsync();

                    return new DocumentationResponse
                    {
                        ConsultationRecordId = existingRecord.Id,
                        ChiefComplaint = existingRecord.ChiefComplaint,
                        HistoryOfPresentIllness = existingRecord.HistoryOfPresentIllness,
                        PhysicalExamination = existingRecord.PhysicalExamination,
                        Diagnosis = existingRecord.Diagnosis,
                        ManagementPlan = existingRecord.ManagementPlan,
                        SessionType = (int)appointment.ConsultationType,
                        CreatedAt = existingRecord.CreatedAt
                    };
                }
                else
                {
                    // Create new record
                    _logger.LogInformation("Creating new consultation record for Appointment {AppointmentId}", appointmentId);

                    var newRecord = new ConsultationRecord
                    {
                        AppointmentId = appointmentId,
                        ChiefComplaint = request.ChiefComplaint ?? string.Empty,
                        HistoryOfPresentIllness = request.HistoryOfPresentIllness ?? string.Empty,
                        PhysicalExamination = request.PhysicalExamination ?? string.Empty,
                        Diagnosis = request.Diagnosis ?? string.Empty,
                        ManagementPlan = request.ManagementPlan ?? string.Empty
                    };

                    await _consultationRecordRepository.AddAsync(newRecord);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Consultation record {RecordId} created successfully", newRecord.Id);

                    return new DocumentationResponse
                    {
                        ConsultationRecordId = newRecord.Id,
                        ChiefComplaint = newRecord.ChiefComplaint,
                        HistoryOfPresentIllness = newRecord.HistoryOfPresentIllness,
                        PhysicalExamination = newRecord.PhysicalExamination,
                        Diagnosis = newRecord.Diagnosis,
                        ManagementPlan = newRecord.ManagementPlan,
                        SessionType = (int)appointment.ConsultationType,
                        CreatedAt = newRecord.CreatedAt
                    };
                }
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error saving documentation for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على توثيق الكشف (Doctor and Patient)
        /// </summary>
        public async Task<DocumentationResponse?> GetDocumentationAsync(Guid appointmentId, Guid userId, bool isDoctor)
        {
            try
            {
                _logger.LogInformation("Getting documentation for Appointment {AppointmentId} by User {UserId} (IsDoctor: {IsDoctor})", 
                    appointmentId, userId, isDoctor);

                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                    return null;
                }

                if (isDoctor)
                {
                    if (appointment.DoctorId != userId)
                    {
                        _logger.LogWarning("Doctor {UserId} tried to access documentation for appointment {AppointmentId} that belongs to another doctor", 
                            userId, appointmentId);
                        throw new UnauthorizedAccessException("This appointment is not for you");
                    }
                }
                else // Patient
                {
                    if (appointment.PatientId != userId)
                    {
                        _logger.LogWarning("Patient {UserId} tried to access documentation for appointment {AppointmentId} that belongs to another patient", 
                            userId, appointmentId);
                        throw new UnauthorizedAccessException("This appointment is not for you");
                    }
                }

                // الحصول على التوثيق
                var record = await _consultationRecordRepository.GetByAppointmentIdAsync(appointmentId);
                if (record == null)
                {
                    _logger.LogInformation("No documentation found for Appointment {AppointmentId}", appointmentId);
                    return null;
                }

                return new DocumentationResponse
                {
                    ConsultationRecordId = record.Id,
                    ChiefComplaint = record.ChiefComplaint,
                    HistoryOfPresentIllness = record.HistoryOfPresentIllness,
                    PhysicalExamination = record.PhysicalExamination,
                    Diagnosis = record.Diagnosis,
                    ManagementPlan = record.ManagementPlan,
                    SessionType = (int)appointment.ConsultationType,
                    CreatedAt = record.CreatedAt
                };
            }
            catch (Exception ex) when (ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Error getting documentation for Appointment {AppointmentId}", appointmentId);
                throw;
            }
        }
    }
}
